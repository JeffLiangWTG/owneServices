using System.Data;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Moq;

namespace Enterprise.Client.JAS.Business.Cognos.Testing
{
	class CognosLineSorterBaseTestCase : TestCaseWithFactory
	{
		protected void AddNewCognosGroupingFlagsAndSignage(ZGuid cognosAccountPK, ZByte mode, ZByte branch, ZByte businessType, ZByte geographical, ZString company)
		{
			CognosGroupingFlags groupingFlags = Factory.New<CognosGroupingFlags>();
			groupingFlags.T4_AJ = cognosAccountPK;
			groupingFlags.T4_Mode = mode;
			groupingFlags.T4_Branch = branch;
			groupingFlags.T4_BusinessType = businessType;
			groupingFlags.T4_Geographical = geographical;
			groupingFlags.T4_Company = company;
		}

		protected CognosLineBizO GetCognosLine(ZGuid cognosAccountPK, ZString mode, ZString branch, ZString businessType, ZString geographical, ZString counterCompany, ZString transactionCurrency)
		{
			var cognosLineMock = new Mock<CognosLineBizO>(new object[] { Factory, new DataTable().NewRow() });
			cognosLineMock.CallBase = true;

			cognosLineMock.Setup(m => m.AccountPK).Returns(cognosAccountPK);
			cognosLineMock.Setup(m => m.Mode).Returns(mode);
			cognosLineMock.Setup(m => m.Branch).Returns(branch);
			cognosLineMock.Setup(m => m.Business).Returns(businessType);
			cognosLineMock.Setup(m => m.Geographical).Returns(geographical);
			cognosLineMock.Setup(m => m.AccountName).Returns((ZString)" ");
			cognosLineMock.Setup(m => m.Amount).Returns(ZDecimal.Zero);
			cognosLineMock.Setup(m => m.AccountCode).Returns(ZString.Empty);
			cognosLineMock.Setup(m => m.CounterCompany).Returns(counterCompany);
			cognosLineMock.Setup(m => m.TransactionAmount).Returns(ZDecimal.Zero);
			cognosLineMock.Setup(m => m.TransactionCurrency).Returns(transactionCurrency);

			return cognosLineMock.Object;
		}

		protected CognosLineBizO GetCognosLine(ZGuid cognosAccountPK, ZString mode, ZString branch, ZString businessType, ZString geographical)
		{
			return GetCognosLine(cognosAccountPK, mode, branch, businessType, geographical, "", "");
		}
	}
}
