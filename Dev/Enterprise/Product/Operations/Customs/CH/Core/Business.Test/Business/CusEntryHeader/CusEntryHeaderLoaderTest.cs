using System.Runtime.CompilerServices;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.CH;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(CusEntryHeader.Loader))]
sealed class CusEntryHeaderLoaderTest : LoaderTestCase
{
	public void TestFindByEntryNumber() => CombineAssertions(() =>
	{
		const string gdrn1 = "24CH202410090007J5";
		const string gdrn2 = "24CH202410090008J4";

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = CHJobMessageTypeList.Codes.Import;
		var entryHeader1 = declaration.CustomsEntryHeaders.AddNew();
		entryHeader1.MovementReferenceNumberSetter(gdrn1);
		var entryHeader2 = declaration.CustomsEntryHeaders.AddNew();
		entryHeader2.MovementReferenceNumberSetter(gdrn2 + ".1");
		Factory.Save();

		var loader = new CusEntryHeader.Loader(Factory);

		AssertFindByGDRN(true, entryHeader1, gdrn1, anyVersion: false);
		AssertFindByGDRN(false, entryHeader1, gdrn1 + ".1", anyVersion: false);
		AssertFindByGDRN(true, entryHeader1, gdrn1 + ".1", anyVersion: true);

		AssertFindByGDRN(false, entryHeader2, gdrn2, anyVersion: false);
		AssertFindByGDRN(true, entryHeader2, gdrn2 + ".1", anyVersion: false);
		AssertFindByGDRN(false, entryHeader2, gdrn2 + ".2", anyVersion: false);
		AssertFindByGDRN(true, entryHeader2, gdrn2 + ".2", anyVersion: true);

		AssertNotNull("entryType: match", loader.FindByEntryNumber(gdrn1, entryType: CusEntryNumberTypes.Standard.MovementReferenceNumber));
		AssertNull("entryType: no match", loader.FindByEntryNumber(gdrn1, entryType: CusEntryNumberTypes.Standard.LocalReferenceNumber));

		AssertNotNull("jobMessageType: match", loader.FindByEntryNumber(gdrn1, jobMessageType: CHJobMessageTypeList.Codes.Import));
		AssertNull("jobMessageType: no match", loader.FindByEntryNumber(gdrn1, jobMessageType: CHJobMessageTypeList.Codes.Export));

		AssertNull("excludeJobDeclarationPK: match", loader.FindByEntryNumber(gdrn1, excludeJobDeclarationPK: declaration.PK));
		AssertNotNull("excludeJobDeclarationPK: no match", loader.FindByEntryNumber(gdrn1, excludeJobDeclarationPK: ZGuid.NewZGuid()));

		void AssertFindByGDRN(bool expected, CusEntryHeader entryHeader, string gdrn, bool anyVersion, [CallerLineNumber] int line = 0)
		{
			var foundEntryHeader = loader.FindByEntryNumber(gdrn, anyVersion: anyVersion);
			AssertEquals($"[{line}] gdrn={gdrn} anyVersion={anyVersion}", expected, entryHeader.Equals(foundEntryHeader));
		}
	});

	public void TestFindByBGMReference() => CombineAssertions(() =>
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader1 = declaration.CustomsEntryHeaders.AddNew();
		var entryHeader2 = declaration.CustomsEntryHeaders.AddNew();
		entryHeader1.CH_BGMReference = "BGM1";
		entryHeader2.CH_BGMReference = "BGM2";
		Factory.Save();

		var loader = new CusEntryHeader.Loader(Factory);

		AssertEquals("Matching BGM1", entryHeader1, loader.FindByBGMReference("BGM1"));
		AssertEquals("Matching BGM2", entryHeader2, loader.FindByBGMReference("BGM2"));
		AssertNull("Unknown BGM", loader.FindByBGMReference("BGMX"));
	});

	protected override BusinessObject.Loader GetNewLoaderToTest() => new CusEntryHeader.Loader(Factory);
}
