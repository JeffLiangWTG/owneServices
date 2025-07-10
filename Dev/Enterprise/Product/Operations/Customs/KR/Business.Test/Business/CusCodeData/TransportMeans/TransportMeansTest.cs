using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.KR;
using Enterprise.Customs.KR.Messaging;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(TransportMeans))]
	sealed class TransportMeansTest : CusCodeDataTest<TransportMeans>
	{
		public void TestSetDefaultValues()
		{
			AssertEquals(CusCodeDataTypeList.Codes.TransportMean, transportMean.CY_Type);
		}

		public void TestValidation()
		{
			AssertType<TransportMeansValidation>(transportMean.Validation);
		}

		public void TestCY_Data()
		{
			transportMean.CY_Data = "22나1234";
			AssertEquals("22나1234", transportMean.CY_Data);
			AssertEquals(20, transportMean.CY_DataInfo.MaxLength);
		}

		public void TestDescription()
		{
			var vessel1 = RefVessel.New(Factory);
			vessel1.RV_Code = "CY_Code Test1";
			vessel1.RV_MalaysiaVesselId = "Data";

			var vessel2 = RefVessel.New(Factory);
			vessel2.RV_Code = "CY_Code Test2";
			vessel2.RV_MalaysiaVesselId = ZString.Empty;

			transportMean.CY_Code = vessel1.RV_Code;
			AssertEquals("Data", transportMean.Description);

			transportMean.CY_Code = vessel2.RV_Code;
			AssertEquals(ZString.Empty, transportMean.Description);

			transportMean.CY_Code = "Error";
			AssertEquals(ZString.Empty, transportMean.Description);
		}

		protected override IEnumerable<TransportMeans> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			yield return transportMean;
		}

		protected override BusinessObject GetNewBusinessObject() => Factory.New<TransportMeans>();

		protected override void SetUp()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.LocalExport;
			transportMean = declaration.TransportMeans.AddNew();
			Factory.Save();
		}
		TransportMeans transportMean;
	}
}
