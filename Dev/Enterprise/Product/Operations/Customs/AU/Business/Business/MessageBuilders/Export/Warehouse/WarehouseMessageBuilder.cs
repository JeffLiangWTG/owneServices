using System.Collections;
using System.Linq;
using CargoWise.Types;
using Enterprise.Edifact.D99B.Elements;
using Enterprise.Edifact.D99B.Messages.CUSCAR;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public abstract class WarehouseMessageBuilder : CMRCUSCARMessageBuilder
	{
		public WarehouseMessageBuilder(ZString eDN, WarehouseItemWrapper[] items)
		{
			this.eDN = eDN;
			this.items = items;
		}

		protected internal override void GenerateMessageText()
		{
			if (CUSCAR == null)
			{
				CUSCAR = new CUSCARMessage();
				PopulateUNH();
				PopulateBGM();
				PopulateLOCs();
				PopulateDTM();
				PopulateSegmentGroup1();
				PopulateSegmentGroup7s();
				PopulateUNT();
			}
		}

		public static WarehouseItemWrapper[] GetWarehouseItems(JobDeclaration declaration)
		{
			ArrayList result = new ArrayList();
			foreach (var invoiceLine in declaration.InvoiceLines.Cast<JobComInvoiceLine>())
			{
				result.Add(new JobComInvoiceLineWarehouseItemWrapper(invoiceLine));
			}

			return (WarehouseItemWrapper[])result.ToArray(typeof(WarehouseItemWrapper));
		}

		public static WarehouseItemWrapper[] GetWarehouseItems(CusEntryHeader entryHeader)
		{
			ArrayList result = new ArrayList();
			foreach (var entryLine in entryHeader.MergedLines.Cast<CusEntryLine>())
			{
				result.Add(new CusEntryLineWarehouseItemWrapper(entryLine));
			}

			return (WarehouseItemWrapper[])result.ToArray(typeof(WarehouseItemWrapper));
		}

		#region Implementation

		protected abstract void PopulateDTM();

		protected abstract void PopulateLOCs();

		protected void PopulateSegmentGroup1()
		{
			if (!eDN.IsEmpty)
			{
				MessageUtilities.PopulateRFF(CUSCAR.Group1[0].RFF[0], ReferenceFunctionCodeQualifierList.ExportDeclaration, eDN, null);
			}
		}

		protected void PopulateSegmentGroup7s()
		{
			int group7Number = 0;
			foreach (WarehouseItemWrapper item in items)
			{
				SegmentGroup7 group7 = CUSCAR.Group7[group7Number];
				MessageUtilities.PopulateCNI(group7.CNI[0], (group7Number + 1).ToString(), null);
				SegmentGroup8 group8 = group7.Group8[0];
				MessageUtilities.PopulateRFF(group8.RFF[0], ReferenceFunctionCodeQualifierList.HarmonisedSystemNumber, item.AHECCCode.KeepChars("1234567890"), null);
				SegmentGroup13 group13 = group8.Group13[0];
				group13.QTY[0].QuantityDetails.QuantityTypeCodeQualifier = QuantityTypeCodeQualifierList.DiscreteQuantity;
				group13.QTY[0].QuantityDetails.Quantity = item.NetQuantity.ToString(5);
				group13.QTY[0].QuantityDetails.MeasurementUnitCode = item.NetQuantityUnit;
				MessageUtilities.PopulateFTX(group13.FTX[0], TextSubjectCodeQualifierList.GoodsDescription, item.GoodsDescription);
				group8.Group14[0].GID[0].GoodsItemNumber = "1";//Trigger
				group7Number++;
			}
		}

		protected WarehouseItemWrapper[] items;

		protected ZString eDN;
		#endregion
	}
}
