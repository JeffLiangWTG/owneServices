using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using CargoWise.Customs.DE.MessageContracts;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.EU.Business;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.AESVersion3_0.Testing
{
	[TestedType(typeof(EXPENTMessageHeaderProvider))]
	class EXPENTMessageHeaderProviderTest : AESMessageHeaderProviderAbstractTest<EXPENTMessageHeaderProvider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>(() => new EXPENTMessageHeaderProvider(null));
		}

		public void TestAESHeader()
		{
			AssertType<EXPENTHeaderProvider>(Provider.AESHeader);
		}

		public void TestInterchangeRecipientID()
		{
			var customsOffice = declaration.CustomsOffices.AddNew();
			customsOffice.CY_Code = EuOfficeCodesTypes.Codes.SupplementaryDeclarationOffice;
			customsOffice.CY_Data = "OFFICE1";
			AssertEquals("OFFICE1", Provider.InterchangeRecipientID);
		}

		protected override IEnumerable<Expression<Func<EXPENTMessageHeaderProvider, object>>> GetPropertiesNeedToBeCached()
		{
			yield return x => x.AESHeader;
		}

		protected override void SetUp()
		{
			base.SetUp();
			action = new ExportEntryMessageSendingAction(entryHeader);
		}
		protected ExportEntryMessageSendingAction action;

		protected override EXPENTMessageHeaderProvider GetProvider() => new EXPENTMessageHeaderProvider(action);

		new IEXPENTMessageHeader Provider => base.Provider;
	}
}
