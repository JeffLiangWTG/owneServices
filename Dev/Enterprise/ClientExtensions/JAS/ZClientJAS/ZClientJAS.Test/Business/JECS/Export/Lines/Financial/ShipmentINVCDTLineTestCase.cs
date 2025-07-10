using CargoWise.Types;
using Enterprise.Client.JAS.Business.Invoicing;
using Enterprise.Freight.Business;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.JAS.Business.JXC.Export.Testing
{
	internal abstract class ShipmentINVCDTLineTestCase : INVCDTLineTestCase
	{
		#region TestLineAsString
		protected override void SetupInvoiceWrapperForTestLineAsString()
		{
			base.SetupInvoiceWrapperForTestLineAsString();
			InvoiceWrapper.Shipment.Consols[0].JK_UniqueConsignRef = "CON101";
			InvoiceWrapper.Shipment.JS_E_DEP = new ZDateTime(2005, 12, 10);
			InvoiceWrapper.Shipment.JS_OuterPacks = 256;
			InvoiceWrapper.Shipment.JS_ActualWeight = 105;
			InvoiceWrapper.Shipment.JS_UnitOfWeight = Core.Constants.Weight.Kilograms;
			InvoiceWrapper.Shipment.JS_ActualVolume = 99;
			InvoiceWrapper.Shipment.JS_UnitOfVolume = Core.Constants.Volume.CubicMetres;
			InvoiceWrapper.Shipment.JS_ActualChargeable = 143;
		}

		protected override ZString[] GetExpectedLineContentForTestLineAsString()
		{
			ZString[] result = base.GetExpectedLineContentForTestLineAsString();
			result[ExpectedFieldPositions.ManifestSerialNumber] = "CON101";
			result[ExpectedFieldPositions.DateCargoManifest] = "10/12/2005";
			result[ExpectedFieldPositions.NumberOfPieces] = "256";
			result[ExpectedFieldPositions.GrossWeight] = "105";
			result[ExpectedFieldPositions.ChargeableWeight] = "143";
			result[ExpectedFieldPositions.Volume] = "99";
			return result;
		}

		#endregion
		#region TestLineAsString_ShipmentHasNoConsol
		[TestDate(2006, 10, 2)]
		public void TestLineAsString_ShipmentHasNoConsol()
		{
			SetupInvoiceWrapperForTestLineAsString_ShipmentHasNoConsol();
			ZString[] lineContent = GetExpectedLineContentAsStringArray_ShipmentHasNoConsol();
			ZString expectedLineAsString = ExpectedLineType + JXCConstants.Version + JXCConstants.Delimiter + ZString.Join(JXCConstants.Delimiter.ToString(), lineContent);
			AssertEquals("LineAsString", expectedLineAsString, Line.LineAsString);
		}

		void SetupInvoiceWrapperForTestLineAsString_ShipmentHasNoConsol()
		{
			Invoice.AH_TransactionNum = "TRAN101";
			Invoice.AH_InvoiceDate = new ZDateTime(2006, 10, 10);
			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);
			InvoiceWrapper.Shipment.Consols.RemoveAll();
		}

		protected virtual ZString[] GetExpectedLineContentAsStringArray_ShipmentHasNoConsol()
		{
			ZString[] result = new ZString[ExpectedFieldCount];
			result[ExpectedFieldPositions.TransactionNumber] = "TRAN101";
			result[ExpectedFieldPositions.TransactionDate] = "10/10/2006";
			result[ExpectedFieldPositions.ISOCountryCode] = "AU";
			result[ExpectedFieldPositions.VATCode] = "GST";
			result[ExpectedFieldPositions.TransactionPerOperativo] = ExpectedLineType.Left(1);
			result[ExpectedFieldPositions.WeightUnit] = "K";
			result[ExpectedFieldPositions.CurrencyCode] = "AUD";
			result[ExpectedFieldPositions.TotalTransaction] = "0";
			result[ExpectedFieldPositions.NumberOfPieces] = "0";
			result[ExpectedFieldPositions.GrossWeight] = "0";
			result[ExpectedFieldPositions.Volume] = "0";
			result[ExpectedFieldPositions.ChargeableWeight] = "0";
			result[ExpectedFieldPositions.DateCargoManifest] = "02/10/2006";
			result[ExpectedFieldPositions.ShipperName] = GlbBranch.CurrentBranch.OrgProxy.OH_FullNameTruncated.SubstringSafe(0, JXCConstants.INVCDTFieldBoundaries.ShipperNameMaxLength);
			result[ExpectedFieldPositions.ShipperAddress1] = GlbBranch.CurrentBranch.OrgProxy.MainAddress.OA_Address1.SubstringSafe(0, JXCConstants.INVCDTFieldBoundaries.ShipperAddressMaxLength);
			result[ExpectedFieldPositions.ShipperAddress2] = GlbBranch.CurrentBranch.OrgProxy.MainAddress.OA_Address2.SubstringSafe(0, JXCConstants.INVCDTFieldBoundaries.ShipperAddressMaxLength);
			return result;
		}

		#endregion
		protected void AddShipmentToInvoice()
		{
			if (Invoice.Job == null)
			{
				Invoice.AH_JH = Factory.NewJobForTesting<JobHeader>().PK;
			}

			Invoice.Job.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			JASForwardingShipment shipment = Factory.New<JASForwardingShipment>();
			shipment.JS_TransportMode = ShipmentTransportMode;
			shipment.JS_RL_NKOrigin = "";
			shipment.JS_RL_NKDestination = "";
			shipment.Consols.AddNew();
			Invoice.Job.JH_ParentID = shipment.PK;
		}

		protected override InvoiceWrapper GetNewInvoiceWrapper()
		{
			InvoiceWrapper result = base.GetNewInvoiceWrapper();
			AddShipmentToInvoice();
			return result;
		}

		protected abstract ZString ShipmentTransportMode { get; }
	}
}
