using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;
using UniversalCodeDescriptionPair = Enterprise.UniversalDataBuss.DataObjects.Universal.CodeDescriptionPair;

namespace Enterprise.DocumentVisualizer.Testing
{
	[TestedType(typeof(CodeDescription))]
	sealed class CodeDescriptionTest : NonPersistentBusinessObjectTestCase
	{
		#region TestUpdateDescriptionWhenCodeChanges_CodeDescription

		public void TestUpdateDescriptionWhenCodeChanges_CodeDescription()
		{
			var list = new CodeDescriptionPairList();
			list.AddPair("AAA", "AAA desc");
			list.AddPair("BBB", "BBB desc");
			list.AddPair("CCC", "CCC desc");

			var codeDescription = new CodeDescription(list);

			AssertEquals("", codeDescription.Code);
			AssertEquals("", codeDescription.Description);

			codeDescription.Code = "BBB";

			AssertEquals("BBB", codeDescription.Code);
			AssertEquals("BBB desc", codeDescription.Description);

			codeDescription.Code = "AAA";

			AssertEquals("AAA", codeDescription.Code);
			AssertEquals("AAA desc", codeDescription.Description);
		}

		#endregion

		#region TestUpdateDescriptionWhenCodeChanges_FindBoxProvider

		public void TestUpdateDescriptionWhenCodeChanges_FindBoxProvider()
		{
			var unlocos = new RefUNLOCOCollection(Factory);

			var codeDescription = new CodeDescription(unlocos);

			AssertEquals("", codeDescription.Code);
			AssertEquals("", codeDescription.Description);

			codeDescription.Code = "CRAPO";

			AssertEquals("CRAPO", codeDescription.Code);
			AssertEquals("Pital Con Desvio", codeDescription.Description);

			codeDescription.Code = "USHIT";

			AssertEquals("USHIT", codeDescription.Code);
			AssertEquals("Highpoint", codeDescription.Description);
		}

		#endregion

		#region TestCreate

		public void TestCreate()
		{
			var universalCodeDescription = new UniversalCodeDescriptionPair
			{
				Code = "AAA",
				Description = "AAA description"
			};

			var codeDesc = CodeDescription.Create(universalCodeDescription);

			AssertEquals(nameof(codeDesc.Code), "AAA", codeDesc.Code);
			AssertEquals(nameof(codeDesc.Description), "AAA description", codeDesc.Description);
		}

		public void TestCreate_Null()
		{
			var codeDesc = CodeDescription.Create(null);

			AssertEquals(nameof(codeDesc.Code), ZString.Empty, codeDesc.Code);
			AssertEquals(nameof(codeDesc.Description), ZString.Empty, codeDesc.Description);
		}

		#endregion

		#region GetNewBusinessObject

		protected override BusinessObject GetNewBusinessObject()
		{
			var list = new CodeDescriptionPairList();
			return new CodeDescription(list);
		}

		#endregion
	}
}
