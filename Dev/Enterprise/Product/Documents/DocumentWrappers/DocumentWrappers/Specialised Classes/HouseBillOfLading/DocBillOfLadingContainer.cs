using System.Collections.Generic;
using System.Text;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.DocumentWrappers
{
	[AllowNoStaticNew]
	public class DocBillofLadingContainer : DocumentWrapper
	{
		#region Construction

		protected DocBillofLadingContainer(DocContainer docContainer)
		{
			this.Container = docContainer;
			if (Container != null)
			{
				DeliveryMode = Container.DeliveryModeDescription;
			}
		}

		public static DocBillofLadingContainer New(DocContainer docContainer)
		{
			DocBillofLadingContainer result = null;

			var overridden = OverridableNewDelegate.Value;
			if (overridden != null)
			{
				result = overridden(docContainer);
			}
			else if (docContainer != null)
			{
				result = new DocBillofLadingContainer(docContainer);
			}

			return result;
		}

		protected delegate DocBillofLadingContainer NewDelegate(DocContainer docContainer);
		protected static readonly Overridable<NewDelegate> OverridableNewDelegate = new Overridable<NewDelegate>();

		public DocContainer Container { get; private set; }

		#endregion

		#region Properties

		/// <summary>
		/// In KG always.
		/// </summary>
		public ZDecimal Weight { get; internal set; }
		public ZString WeightUQ { get { return Constants.Weight.Kilograms; } }

		/// <summary>
		/// In M3 always.
		/// </summary>
		public ZDecimal Volume { get; internal set; }
		public ZString VolumeUQ { get { return Constants.Volume.CubicMetres; } }

		public ZInt Packs { get; internal set; }

		public ZString PackType { get; internal set; }

		public ZString DeliveryMode
		{
			get { return deliveryMode.IsEmpty ? "-" : deliveryMode.ToString(); }
			set { deliveryMode = value.StartsWith("CY") ? (ZString)(value + "*") : value; }
		}
		ZString deliveryMode;

		public ZString ContainerMode { get; internal set; }

		public ZString Temperature
		{
			get
			{
				if (Container == null || !Container.IsControlledAtmosphere)
				{
					return ZString.Empty;
				}
				else
				{
					return ZString.Format("{0:0.#}{1}", Container.SetPointTemp, Container.SetPointTempUnit);
				}
			}
		}

		public ZString Humidity
		{
			get
			{
				if (Container == null || Container.HumidityPercent.IsEmpty)
				{
					return ZString.Empty;
				}
				else
				{
					return Container.HumidityPercent + '%';
				}
			}
		}

		public ZString ContainerNumber
		{
			get { return Container != null ? Container.ContainerNumber : ZString.Empty; }
		}

		public ZString ContainerCode
		{
			get { return Container != null ? Container.ContainerCode : ZString.Empty; }
		}

		public ZString ContainerSeal
		{
			get
			{
				ZString result = new();

				if (!Container.SealNumber.IsEmpty)
				{
					result = Container.SealNumber;
				}

				if (!Container.SealNumber2.IsEmpty)
				{
					result += ", " + Container.SealNumber2;
				}

				if (!Container.SealNumber3.IsEmpty)
				{
					result += ", " + Container.SealNumber3;
				}

				result = result.TrimStart(',').TrimStart(' ');

				return result.IsEmpty ? (ZString)"-" : result;
			}
		}

		public ZString ContainerType
		{
			get { return Container.Container == null ? "-" : Container.Container.Code.ToString(); }
		}

		public ZDecimal TotalAllocatedShipmentWeight
		{
			get { return Container.TotalAllocatedShipmentWeight; }
		}

		public ZDecimal TotalAllocatedShipmentVolume
		{
			get { return Container.TotalAllocatedShipmentVolume; }
		}

		public ZString TotalAllocatedShipmentVolumeUQ
		{
			get { return Container.TotalAllocatedShipmentVolumeUQ; }
		}

		public ZDecimal ContainerGross
		{
			get { return ContainerTare + Weight; }
		}

		public ZDecimal ContainerTare
		{
			get { return Core.Constants.Weight.ConvertSafe(Container.TareWeight, Container.TareWeightUQ, Core.Constants.Weight.Kilograms); }
		}

		public ZDecimal ContainerGrossAsEntered
		{
			get { return ContainerTareAsEntered + Container.NetWeight; }
		}

		public ZDecimal ContainerTareAsEntered
		{
			get { return Container.TareWeight; }
		}

		public ZString ContainerWeightUQAsEntered
		{
			get { return Container.WeightUQ; }
		}

		public ZDecimal ContainerVolumeAsEntered
		{
			get { return Container.TotalVolume; }
		}

		public ZString ContainerVolumeUQAsEntered
		{
			get { return Container.TotalVolumeUnit; }
		}

		public TextSection GetPackingDetailsBreakdown(int maxLineLength)
		{
			const int padding = 1;
			const string separator = " - ";

			TextSection result = new TextSection(maxLineLength);

			DocShipment shipmentWrapper = Container.Shipment;

			foreach (DocPackLines packLine in Container.AllocatedShipmentPackLine)
			{
				var packType = packLine.PackageCount.ToString().PadLeft(5) + " " + packLine.PackType;

				var lines = new List<string>();
				lines.Add(packType);
				lines.Add(packLine.ActualWeight.ToStringTrimZeros() + " " + packLine.ActualWeightUQ);

				if (shipmentWrapper.ShowPacklineVolume)
				{
					lines.Add(packLine.ActualVolume.ToStringTrimZeros() + " " + packLine.ActualVolumeUQ);
				}

				if (packLine.Commodity != null)
				{
					if (shipmentWrapper.ShowPacklineCommodityCode)
					{
						AddIfNotEmpty(lines, packLine.Commodity.Code);
					}

					if (shipmentWrapper.ShowPacklineCommodityDescription)
					{
						AddIfNotEmpty(lines, packLine.Commodity.Description);
					}
				}

				AddIfNotEmpty(lines, GetHazardousDescription(packLine));
				AddIfNotEmpty(lines, packLine.Description.Replace("\n", " "));

				if (shipmentWrapper.ShowPacklineMarksAndNumbers)
				{
					AddIfNotEmpty(lines, packLine.MarksAndNumbers.Replace("\n", " "));
				}

				result.Add(ZString.Replicate(' ', padding) + string.Join(separator, lines.ToArray()));
			}

			return result;
		}

		ZString GetHazardousDescription(DocPackLines line)
		{
			if (line.UNDGs.Length == 0)
			{
				return ZString.Empty;
			}
			else
			{
				StringBuilder builder = new StringBuilder();
				builder.Append(line.UNDGs[0].Summary);

				for (int i = 1; i < line.UNDGs.Length; i++)
				{
					builder.Append(System.Environment.NewLine);
					builder.Append(line.UNDGs[i].Summary);
				}

				var helper = new UNDGSubstanceWrapperHelper();
				var summary = helper.GetUNDGPackagesSummary(line.UNDGs);
				if (!summary.IsEmpty)
				{
					builder.Append(System.Environment.NewLine);
					builder.Append(summary);
				}

				return builder.ToString();
			}
		}

		void AddIfNotEmpty(List<string> list, ZString value)
		{
			if (!value.IsEmpty)
			{
				list.Add(value);
			}
		}

		public override string ToString()
		{
			return GetType().ToString();
		}

		#endregion
	}
}
