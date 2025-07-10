using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class PackageTypeConverterTest : TestCaseWithFactory
	{
		public void TestGetCustomsPackageType()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_TotalNoOfPacksPackType = string.Empty;
			var shipment = Factory.New<ForwardingShipment>();
			declaration.JE_JS = shipment.PK;
			var synchroniser = new JobDeclarationSynchroniser(declaration);

			synchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Start));
			shipment.JS_F3_NKPackType = "";
			AssertEquals("This is Empty", string.Empty, declaration.JE_TotalNoOfPacksPackType);

			shipment.JS_F3_NKPackType = "AAA";
			AssertEquals("declaration.JE_TotalNoOfPacksPackType is not updated.", string.Empty, declaration.JE_TotalNoOfPacksPackType);

			shipment.JS_F3_NKPackType = "BBL";
			AssertEquals("declaration.JE_TotalNoOfPacksPackType is updated.", "BA", declaration.JE_TotalNoOfPacksPackType);

			shipment.JS_F3_NKPackType = "BND";
			AssertEquals("declaration.JE_TotalNoOfPacksPackType is updated.", "BE", declaration.JE_TotalNoOfPacksPackType);

			shipment.JS_F3_NKPackType = "BAG";
			AssertEquals("declaration.JE_TotalNoOfPacksPackType is updated.", "BG", declaration.JE_TotalNoOfPacksPackType);

			shipment.JS_F3_NKPackType = "BLC";
			AssertEquals("declaration.JE_TotalNoOfPacksPackType is updated.", "BL", declaration.JE_TotalNoOfPacksPackType);

			shipment.JS_F3_NKPackType = "BOT";
			AssertEquals("declaration.JE_TotalNoOfPacksPackType is updated.", "BV", declaration.JE_TotalNoOfPacksPackType);

			shipment.JS_F3_NKPackType = "CAN";
			AssertEquals("declaration.JE_TotalNoOfPacksPackType is updated.", "CA", declaration.JE_TotalNoOfPacksPackType);

			shipment.JS_F3_NKPackType = "COI";
			AssertEquals("declaration.JE_TotalNoOfPacksPackType is updated.", "CL", declaration.JE_TotalNoOfPacksPackType);

			shipment.JS_F3_NKPackType = "CRT";
			AssertEquals("declaration.JE_TotalNoOfPacksPackType is updated.", "CR", declaration.JE_TotalNoOfPacksPackType);

			shipment.JS_F3_NKPackType = "PWB";
			AssertEquals("declaration.JE_TotalNoOfPacksPackType is updated.", "CR", declaration.JE_TotalNoOfPacksPackType);

			shipment.JS_F3_NKPackType = "CAS";
			AssertEquals("declaration.JE_TotalNoOfPacksPackType is updated.", "CS", declaration.JE_TotalNoOfPacksPackType);

			shipment.JS_F3_NKPackType = "BOX";
			AssertEquals("declaration.JE_TotalNoOfPacksPackType is updated.", "CT", declaration.JE_TotalNoOfPacksPackType);
			shipment.JS_F3_NKPackType = "CTN";
			AssertEquals("declaration.JE_TotalNoOfPacksPackType is updated.", "CT", declaration.JE_TotalNoOfPacksPackType);
			shipment.JS_F3_NKPackType = "CNT";
			AssertEquals("declaration.JE_TotalNoOfPacksPackType is updated.", "CT", declaration.JE_TotalNoOfPacksPackType);
			shipment.JS_F3_NKPackType = "DOZ";
			AssertEquals("declaration.JE_TotalNoOfPacksPackType is updated.", "CT", declaration.JE_TotalNoOfPacksPackType);

			shipment.JS_F3_NKPackType = "DRM";
			AssertEquals("declaration.JE_TotalNoOfPacksPackType is updated.", "DR", declaration.JE_TotalNoOfPacksPackType);

			shipment.JS_F3_NKPackType = "SHT";
			AssertEquals("declaration.JE_TotalNoOfPacksPackType is updated.", "GT", declaration.JE_TotalNoOfPacksPackType);
			shipment.JS_F3_NKPackType = "UNT";
			AssertEquals("declaration.JE_TotalNoOfPacksPackType is updated.", "GT", declaration.JE_TotalNoOfPacksPackType);
			shipment.JS_F3_NKPackType = "EA";
			AssertEquals("declaration.JE_TotalNoOfPacksPackType is updated.", "GT", declaration.JE_TotalNoOfPacksPackType);
			shipment.JS_F3_NKPackType = "PCE";
			AssertEquals("declaration.JE_TotalNoOfPacksPackType is updated.", "GT", declaration.JE_TotalNoOfPacksPackType);
			shipment.JS_F3_NKPackType = "PCS";
			AssertEquals("declaration.JE_TotalNoOfPacksPackType is updated.", "GT", declaration.JE_TotalNoOfPacksPackType);

			shipment.JS_F3_NKPackType = "TRY";
			AssertEquals("declaration.JE_TotalNoOfPacksPackType is updated.", "PU", declaration.JE_TotalNoOfPacksPackType);

			shipment.JS_F3_NKPackType = "REL";
			AssertEquals("declaration.JE_TotalNoOfPacksPackType is updated.", "RL", declaration.JE_TotalNoOfPacksPackType);

			shipment.JS_F3_NKPackType = "RLL";
			AssertEquals("declaration.JE_TotalNoOfPacksPackType is updated.", "RO", declaration.JE_TotalNoOfPacksPackType);

			shipment.JS_F3_NKPackType = "BBG";
			AssertEquals("declaration.JE_TotalNoOfPacksPackType is updated.", "VO", declaration.JE_TotalNoOfPacksPackType);

			shipment.JS_F3_NKPackType = "BBK";
			AssertEquals("declaration.JE_TotalNoOfPacksPackType is updated.", "VT", declaration.JE_TotalNoOfPacksPackType);
		}
	}
}

