using System.IO;
using System.Text;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.DataInterface.ChinaDataInterface
{
	public abstract class FileOutputTest : TestCaseWithFactory
	{
		public void TestOutput()
		{
			using (MemoryStream testStream = new MemoryStream())
			{
				StreamWriter testWriter = new StreamWriter(testStream);
				OutputToTest = GetOutputToTest();
				SetValuesBeforeAssert();
				OutputToTest.Write(testWriter);
				testWriter.Close();
				byte[] byteReSult = testStream.ToArray();
				UTF8Encoding enc = new UTF8Encoding();
				string result = enc.GetString(byteReSult);
				string expectedResult = SetExpectedResult();
				AssertEquals(expectedResult, result);
			}
		}

		protected abstract void SetValuesBeforeAssert();
		protected abstract IFileOutput GetOutputToTest();
		protected abstract string SetExpectedResult();
		protected string ExpectedResult;
		protected IFileOutput OutputToTest;
		protected AccGLAccountDescriptor AddGLHeaderDescriptor(string accountNum, string accountDescr, string debitCredit)
		{
			return AddGLHeaderDescriptor(ZGuid.Empty, accountNum, accountDescr, debitCredit);
		}

		protected AccGLAccountDescriptor AddGLHeaderDescriptor(ZGuid gLPK, string accountNum, string accountDescr, string debitCredit)
		{
			AccGLAccountDescriptor gLHeaderToAdd = Factory.New(typeof(AccGLAccountDescriptor)) as AccGLAccountDescriptor;
			gLHeaderToAdd.AJ_Language = DataInterfaceUtils.GetLocalLanguage();
			gLHeaderToAdd.AJ_RN_NKCountryOfCompliance = "CN";
			gLHeaderToAdd.AJ_AccountDescription = accountDescr;
			gLHeaderToAdd.AJ_LocalAccountNumber = accountNum;
			gLHeaderToAdd.AJ_DebitCredit = debitCredit;
			gLHeaderToAdd.ParentGLHeaderPK = gLPK;
			return gLHeaderToAdd;
		}
	}
}