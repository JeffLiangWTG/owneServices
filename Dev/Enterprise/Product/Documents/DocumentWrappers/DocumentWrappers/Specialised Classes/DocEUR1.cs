using System;
using System.Collections.Generic;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentWrappers.FormatTables;
using Enterprise.DocumentWrappers.GenericWrappers;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentWrappers
{
	public class DocEUR1 : DocBaseWrapper
	{
		public static DocEUR1 New(BusinessObject bizObj, BusinessObjectFactory factory)
		{
			FreightWrapper wrapper = FreightWrapper.New(bizObj, factory)[0];
			return wrapper == null ? null : new DocEUR1(wrapper, factory);
		}

		DocEUR1(FreightWrapper freight, BusinessObjectFactory factory)
			: base(freight.FreightShipment ?? freight.WrappedObject, factory)
		{
			this.freight = freight;
		}

		#region Related Wrappers

		public FreightWrapper Freight
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return freight; }
		}

		#endregion

		#region ShipmentMarksAndNumbers

		public ZString ShipmentMarksAndNumbers
		{
			get { return Freight.FreightShipment.JS_MarksAndNumbers; }
		}

		#endregion

		#region TotalWeight / TotalVolume

		public ZString TotalWeight
		{
			get
			{
				return Core.Constants.Weight.ConvertSafe(Freight.FreightShipment.JS_ActualWeight, Freight.FreightShipment.JS_UnitOfWeight, Core.Constants.Weight.Kilograms).ToString() +
					" " + Core.Constants.Weight.Kilograms;
			}
		}

		public ZString TotalVolume
		{
			get
			{
				return Core.Constants.Volume.ConvertSafe(Freight.FreightShipment.JS_ActualVolume, Freight.FreightShipment.JS_UnitOfVolume, Core.Constants.Volume.CubicMetres).ToString() +
					" " + Core.Constants.Volume.CubicMetres;
			}
		}

		public ZBool ShipmentHasWeight
		{
			get { return Freight.FreightShipment.JS_ActualWeight != 0; }
		}

		#endregion

		#region FormattedBody

		public ZString FormattedBody
		{
			get { return formattedBody ?? (formattedBody = GenerateFormattedBody()); }
		}
		string GenerateFormattedBody()
		{
			string[] lines = new string[BodyHeight];
			IEnumerator<string> src;
			int totalWidth;

			if (freight != null && freight.FreightShipment != null)
			{
				FormatTable table = FormattedBodyTable((PackLine[])freight.FreightShipment.OuterPackLines.ToArray(typeof(PackLine)), Freight.FreightShipment);

				table = new FormatTable(false,
					new FormatColumn(table.Body, table.CalculateTotalWidth()),
					new FormatColumn(GetDocDataValue((NoResString)"Invoices", "").ToString().Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries), InvoiceWidth) { LeftPadding = MeasureInvoiceGap }
				);

				src = table.Body.GetEnumerator();
				totalWidth = table.CalculateTotalWidth();
			}
			else
			{
				src = null;
				totalWidth = 0;
			}

			for (int i = 0; i < BodyHeight; i++)
			{
				if (src == null)
				{
					if (DiagionalLinesGap > 0)
					{
						StringBuilder builder = new StringBuilder(totalWidth);
						for (int j = 0; j < totalWidth; j++)
						{
							builder.Append((i - j) % DiagionalLinesGap == 0 ? '\\' : ' ');
						}
						lines[i] = builder.ToString();
					}
					else
					{
						lines[i] = new string(' ', totalWidth);
					}
				}
				else if (!src.MoveNext())
				{
					lines[i] = new string('-', totalWidth);
					src = null;
				}
				else
				{
					lines[i] = src.Current;
				}
			}

			return string.Join("\n", lines);
		}
		string formattedBody;

		FormatTable FormattedBodyTable(PackLine[] packlines, ForwardingShipment shipment)
		{
			string[] detail = new string[packlines.Length];
			string[] measure = new string[packlines.Length];
			string[] items = new string[packlines.Length];
			int maxItemNo = 0;

			for (int i = 0; i < packlines.Length; i++)
			{
				PackLine packline = packlines[i];
				if (maxItemNo < packline.JL_ItemNo)
				{
					maxItemNo = packline.JL_ItemNo;
				}

				StringBuilder detailBuilder = new StringBuilder();
				StringBuilder measureBuilder = new StringBuilder();

				detailBuilder.AppendFormat("{0} {1}", packline.JL_PackageCount, packline.JL_F3_NKPackType);

				if (!packline.JL_Description.IsEmpty)
				{
					detailBuilder.Append(' ');
					detailBuilder.Append(packline.JL_Description);
				}

				if (packline.JL_ActualVolume > 0 && shipment.JS_PackingMode != Core.Constants.ContainerModes.FCL)
				{
					if (ShipmentHasWeight && packline.JL_ActualWeight != 0)
					{
						if (measureBuilder.Length > 0)
						{
							measureBuilder.Append('\n');
						}

						measureBuilder.AppendFormat("{0:0.000} {1}", packline.JL_ActualWeight, packline.JL_ActualWeightUQ);
					}
					else if (!ShipmentHasWeight && packline.JL_ActualVolume != 0)
					{
						if (measureBuilder.Length > 0)
						{
							measureBuilder.Append('\n');
						}

						measureBuilder.AppendFormat("{0:0.000} {1}", packline.JL_ActualVolume, packline.JL_ActualVolumeUQ);
					}
				}

				items[i] = packline.JL_ItemNo + ")";
				detail[i] = detailBuilder.ToString();
				measure[i] = measureBuilder.ToString();
			}

			if (maxItemNo == 0)
			{
				return new FormatTable(false,
					new FormatColumn(detail, DetailWidth),
					new FormatColumn(measure, MeasureWidth) { LeftPadding = DetailMeasureGap, Options = FormatColumnOptions.RightAlign }
				);
			}
			else
			{
				int itemsWidth = maxItemNo.ToString().Length + 1;

				return new FormatTable(false,
					new FormatColumn(items, itemsWidth) { Options = FormatColumnOptions.RightAlign },
					new FormatColumn(detail, DetailWidth - itemsWidth - 1) { LeftPadding = 1 },
					new FormatColumn(measure, MeasureWidth) { LeftPadding = DetailMeasureGap, Options = FormatColumnOptions.RightAlign }
				);
			}
		}

		#endregion

		protected override string[] ImageNamesToRemove
		{
			get { return GetTemplateConstantValue<ZString>(DocumentEngineIntegration.Constants.TemplateDefined.ImageNamesToRemove).ToString().Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries); }
		}

		public ZInt DetailWidth
		{
			get { return GetTemplateConstantValue<ZInt>(DocumentEngineIntegration.Constants.TemplateDefined.DetailWidth, 1); }
		}

		public ZInt DetailMeasureGap
		{
			get { return GetTemplateConstantValue<ZInt>(DocumentEngineIntegration.Constants.TemplateDefined.DetailMeasureGap, 1); }
		}

		public ZInt MeasureWidth
		{
			get { return GetTemplateConstantValue<ZInt>(DocumentEngineIntegration.Constants.TemplateDefined.MeasureWidth, 1); }
		}

		public ZInt MeasureInvoiceGap
		{
			get { return GetTemplateConstantValue<ZInt>(DocumentEngineIntegration.Constants.TemplateDefined.MeasureInvoiceGap, 1); }
		}

		public ZInt InvoiceWidth
		{
			get { return GetTemplateConstantValue<ZInt>(DocumentEngineIntegration.Constants.TemplateDefined.InvoiceWidth, 1); }
		}

		public ZInt BodyHeight
		{
			get { return GetTemplateConstantValue<ZInt>(DocumentEngineIntegration.Constants.TemplateDefined.BodyHeight, 1); }
		}

		public ZInt DiagionalLinesGap
		{
			get { return GetTemplateConstantValue<ZInt>(DocumentEngineIntegration.Constants.TemplateDefined.DiagionalLinesGap, 10); }
		}

		readonly FreightWrapper freight;
	}
}
