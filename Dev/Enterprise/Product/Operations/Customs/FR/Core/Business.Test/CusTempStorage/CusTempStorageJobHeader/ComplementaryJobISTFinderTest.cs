using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.FR.Business.Declaration;

namespace Enterprise.Customs.FR.Business.CusTempStorage.Testing
{
	public class ComplementaryJobISTFinderTest : TestCaseWithFactory
	{
		public void TestFindFromPreviousDocuments()
		{
			var ist = CusTempStorageJobHeader.New(Factory, FRConstants.TemporaryStorage.AppCodeIST);
			ist.SJH_JobReference = "FRJ000046";
			var cusEntryNum = CusEntryNumber.New(ist, CusEntryNumberTypes.France.DDT, Core.Constants.CountryCodes.France);
			cusEntryNum.CE_EntryNum = "A000024";

			var otherIst = CusTempStorageJobHeader.New(Factory, FRConstants.TemporaryStorage.AppCodeIST);
			otherIst.SJH_JobReference = "FRJ000089";
			var otherCusEntryNum = CusEntryNumber.New(ist, CusEntryNumberTypes.France.Import, Core.Constants.CountryCodes.France);
			otherCusEntryNum.CE_EntryNum = "A000078";

			var complementaryDocumentCode = "PRE";
			var othercomplementaryDocumentCode = "FOL";

			var pd1 = Factory.New<PreviousDocument>();
			pd1.CSI_Code = complementaryDocumentCode;
			pd1.CSI_ReferenceNumber = "FRJ000046";
			var pd2 = Factory.New<PreviousDocument>();
			pd2.CSI_Code = othercomplementaryDocumentCode;
			pd2.CSI_ReferenceNumber = "FRJ000089";
			var complementaryIST = ComplementaryJobISTFinder.FindFromPreviousDocuments(Factory, new[] { pd1, pd2 }, complementaryDocumentCode, Core.Constants.CountryCodes.France);
			AssertSame("We should find the IST job which reference matches the previous document specified by complementaryDocumentCode.", ist, complementaryIST);

			var pd3 = Factory.New<PreviousDocument>();
			pd3.CSI_Code = complementaryDocumentCode;
			pd3.CSI_ReferenceNumber = "A000024";
			var pd4 = Factory.New<PreviousDocument>();
			pd4.CSI_Code = complementaryDocumentCode;
			pd4.CSI_ReferenceNumber = "A000078";
			complementaryIST = ComplementaryJobISTFinder.FindFromPreviousDocuments(Factory, new[] { pd3, pd4 }, complementaryDocumentCode, Core.Constants.CountryCodes.France);
			AssertSame("We should find the IST job which has entry number DDT.", ist, complementaryIST);
		}

		public void TestFindFromReferenceNumber()
		{
			var ist = CusTempStorageJobHeader.New(Factory, FRConstants.TemporaryStorage.AppCodeIST);
			ist.SJH_JobReference = "FRJ000046";
			var cusEntryNum = CusEntryNumber.New(ist, CusEntryNumberTypes.France.DDT, Core.Constants.CountryCodes.France);
			cusEntryNum.CE_EntryNum = "A000024";

			var otherIst = CusTempStorageJobHeader.New(Factory, FRConstants.TemporaryStorage.AppCodeIST);
			otherIst.SJH_JobReference = "FRJ000089";
			var otherCusEntryNum = CusEntryNumber.New(ist, CusEntryNumberTypes.France.Import, Core.Constants.CountryCodes.France);
			otherCusEntryNum.CE_EntryNum = "A000078";

			var complementaryIST = ComplementaryJobISTFinder.FindFromReferenceNumber(Factory, "FRJ000046", Core.Constants.CountryCodes.France);
			AssertSame("We should find the IST job with reference matched.", ist, complementaryIST);

			complementaryIST = ComplementaryJobISTFinder.FindFromReferenceNumber(Factory, "A000024", Core.Constants.CountryCodes.France);
			AssertSame("We should find the IST job which has entry number DDT.", ist, complementaryIST);
		}
	}
}
