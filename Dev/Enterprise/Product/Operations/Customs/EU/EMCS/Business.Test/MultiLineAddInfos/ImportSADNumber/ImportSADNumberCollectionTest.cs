using CargoWise.ComponentModel;
using Enterprise.Customs.Business;
using Enterprise.Messaging.Business;
using NUnit.Framework;

namespace Enterprise.Customs.EU.EMCS.Business.Testing
{
	[TestedType(typeof(ImportSADNumberCollection<ImportSADNumber>))]
	public class ImportSADNumberCollectionTest : Customs.Business.Testing.CusSupportingInfoCollectionTest<ImportSADNumber>
	{
		public void TestAllowNewCore()
		{
			declaration.JE_MessageStatus = string.Empty;
			Factory.InvalidateCachedProperties();

			CombineAssertions(() =>
			{
				AssertEquals("Allow New", true, importSADNumbers.AllowNew);

				declaration.JE_MessageStatus = EDIMessage.Status.Sent;
				Factory.InvalidateCachedProperties();
				AssertEquals("Should not allow new when the message status of parent is SNT.", false, importSADNumbers.AllowNew);

				declaration.JE_MessageStatus = EDIMessage.Status.Acknowledged;
				Factory.InvalidateCachedProperties();
				AssertEquals("Should not allow new when the message status of parent is ACK.", false, importSADNumbers.AllowNew);
			});
		}

		public void TestTooManyImportSADNumbers()
		{
			for (var i = 0; i < 9; i++)
			{
				importSADNumbers.AddNew();
			}
			CombineAssertions(() =>
			{
				AssertEquals("Maximum allowed", false, importSADNumbers.HasErrors());

				var extraImportSADNumber = importSADNumbers.AddNew();
				AssertHasRowError(extraImportSADNumber, "There are too many Import SAD Numbers. Maximum of 9.");
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<EMCSJobDeclaration>();
			importSADNumbers = new ImportSADNumberCollection<ImportSADNumber>(declaration);
		}

		EMCSJobDeclaration declaration;
		ImportSADNumberCollection importSADNumbers;

		protected override CusSupportingInfoCollection<ImportSADNumber> GetCusSupportingInfoCollection()
		{
			var declaration = Factory.New<EMCSJobDeclaration>();
			return new ImportSADNumberCollection<ImportSADNumber>(declaration);
		}
	}
}
