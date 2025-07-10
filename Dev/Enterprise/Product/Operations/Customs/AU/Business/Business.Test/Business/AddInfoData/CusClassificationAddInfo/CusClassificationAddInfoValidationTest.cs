using CargoWise.ComponentModel;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CusClassificationAddInfoValidationTest : AUAddInfoValidationTest
	{
		public void TestValidateInstrumentCode()
		{
			TestCaseHelper.ClearTable(CMRInstrument.Schema.TableName);

			var classification = Factory.New<Classification>();
			classification.CC_ClassificationType = Classification.ClassificationType.IMP;
			classification.CC_TariffNum = "2001.10.00 90";
			classification.InstrumentType = "BL";
			classification.TreatmentCode = "427";
			classification.InstrumentCode = "9840020";
			AssertEquals("!HasNotifications", true, classification.InstrumentCodeInfo.HasWarnings());

			var instrument = CMRInstrument.New(Factory);
			instrument.IN_Type = "BL";
			instrument.IN_Number = "9840020";
			instrument.IN_StartDate = ZDateTime.Today;

			classification.InstrumentCode = "9840020";
			AssertEquals("!HasNotifications", false, classification.InstrumentCodeInfo.HasWarnings());
		}
	}
}
