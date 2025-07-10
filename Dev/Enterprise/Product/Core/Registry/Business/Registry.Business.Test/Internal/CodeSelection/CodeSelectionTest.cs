using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(CodeSelection))]
	public sealed class CodeSelectionTest : RegistryBusinessObjectTemplateTestCase<CodeSelection>
	{
		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected override CodeSelection GetBusinessObjectToClone()
		{
			BizObj.Code = "GRE";
			return BizObj;
		}

		protected override CodeSelection GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			CodeSelectionCollection collection = new CodeSelectionCollection(GetCodesProviderForTesting());
			return collection.AddNew();
		}

		public static CodeDescriptionPairListProvider GetCodesProviderForTesting()
		{
			return new CodeDescriptionPairListProvider(() =>
			{
				var list = new CodeDescriptionPairList();
				list.AddPair("GRE", "Grenades");
				list.AddPair("RIF", "Rifles");
				return list;
			});
		}

		public void TestDescription()
		{
			BizObj.Code = "";
			AssertEquals("Description", "", BizObj.Description);

			BizObj.Code = "GRE";
			AssertEquals("Description", "Grenades", BizObj.Description);

			BizObj.Code = "RIF";
			AssertEquals("Description", "Rifles", BizObj.Description);
		}

		public void TestValidation()
		{
			BizObj.Code = "";
			AssertHasError(BizObj.CodeInfo, "Please enter a value.");

			BizObj.Code = "ZZZ";
			AssertHasError(BizObj.CodeInfo, "Enter a valid selection.");

			BizObj.Code = "GRE";
			AssertNoErrors(BizObj.CodeInfo);

			CodeSelection anotherSelection = (CodeSelection)BizObj.ParentCollections.First().AddNew();
			anotherSelection.Code = "GRE";
			AssertHasError(anotherSelection.CodeInfo, "The Code has been duplicated and must be unique.");

			anotherSelection.Code = "RIF";
			AssertNoErrors(anotherSelection.CodeInfo);

			BizObj.Code = "";
			BizObj.ClearAllNotifications();
			BizObj.RunPreSaveValidation();
			AssertHasError(BizObj.CodeInfo, "Please enter a value.");
		}
	}
}
