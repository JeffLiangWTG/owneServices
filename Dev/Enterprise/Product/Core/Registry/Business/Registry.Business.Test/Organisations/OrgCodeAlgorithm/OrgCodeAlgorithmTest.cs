using CargoWise.Organizations.CodeGeneration;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(OrgCodeAlgorithm))]
	sealed class OrgCodeAlgorithmTest : RegistryBusinessObjectTemplateTestCase<OrgCodeAlgorithm>
	{
		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		public static void AssertAlgorithmEquals(OrgCodeAlgorithm originalBusinessObject, OrgCodeAlgorithm newBusinessObject, bool isClone)
		{
			AssertEquals("AllowRecalculatedOrgCodeByUser", originalBusinessObject.AllowRecalculatedOrgCodeByUser, newBusinessObject.AllowRecalculatedOrgCodeByUser);
			AssertEquals("RegenerateOrgCodeOnChanges", originalBusinessObject.RegenerateOrgCodeOnChanges, newBusinessObject.RegenerateOrgCodeOnChanges);
			AssertEquals("AlgorithmType", originalBusinessObject.AlgorithmType, newBusinessObject.AlgorithmType);

			AssertEquals("Elements.Parent", newBusinessObject, newBusinessObject.Elements.Parent);
			AssertEquals("Elements should be registered as an editable child object.", true, newBusinessObject.IsRegisteredEditableChildObject(newBusinessObject.Elements));
			AssertEquals("Elements.Count", originalBusinessObject.Elements.Count, newBusinessObject.Elements.Count);
			foreach (OrgCodeElement originalElement in originalBusinessObject.Elements)
			{
				string id = "Elements[\"" + originalElement.Description + "\"]";
				OrgCodeElement newElement = newBusinessObject.Elements[originalElement.Description];
				AssertEquals(id + ".Description", originalElement.Description, newElement.Description);
				AssertEquals(id + ".Length", originalElement.Length, newElement.Length);
				AssertEquals(id + ".Order", originalElement.Order, newElement.Order);
			}

			AssertEquals("SelectableOrgTypes.Parent", newBusinessObject, newBusinessObject.SelectableOrgTypes.Parent);
			AssertEquals("SelectableOrgTypes should be registered as an editable child object.", true, newBusinessObject.IsRegisteredEditableChildObject(newBusinessObject.SelectableOrgTypes));
			AssertEquals("SelectableOrgTypes.Count", originalBusinessObject.SelectableOrgTypes.Count, newBusinessObject.SelectableOrgTypes.Count);
			foreach (OrgCodeOrgType originalSelectableOrgType in originalBusinessObject.SelectableOrgTypes)
			{
				string id = "SelectableOrgTypes[\"" + originalSelectableOrgType.Description + "\"]";
				OrgCodeOrgType newSelectableOrgType = newBusinessObject.SelectableOrgTypes[originalSelectableOrgType.Description];
				AssertEquals(id + ".Description", originalSelectableOrgType.Description, newSelectableOrgType.Description);
				AssertEquals(id + ".Selected", originalSelectableOrgType.Selected, newSelectableOrgType.Selected);
			}
		}

		protected override void CheckAllPropertiesAreEqual(OrgCodeAlgorithm originalBusinessObject, OrgCodeAlgorithm newBusinessObject, bool isClone)
		{
			AssertAlgorithmEquals(originalBusinessObject, newBusinessObject, isClone);
		}

		protected override OrgCodeAlgorithm GetBusinessObjectToClone()
		{
			return GetTestAlgorithm();
		}

		protected override OrgCodeAlgorithm GetBusinessObjectToSerialise()
		{
			return GetTestAlgorithm();
		}

		public static OrgCodeAlgorithm GetTestAlgorithm()
		{
			OrgCodeAlgorithm result = new OrgCodeAlgorithm();
			result.RegenerateOrgCodeOnChanges = true;
			result.AllowRecalculatedOrgCodeByUser = true;
			result.Elements[1].Order = 3;
			result.Elements[1].Length = 5;
			result.Elements[3].Order = 6;
			result.Elements[3].Length = 2;
			result.SelectableOrgTypes[OrgCodeOrgTypeDescription.Consignor].Selected = true;
			result.SelectableOrgTypes[OrgCodeOrgTypeDescription.Carrier].Selected = true;
			return result;
		}

		public void TestAlgorithmType()
		{
			AssertEquals("AlgorithmType", OrgCodeAlgorithmType.Undefined, BizObj.AlgorithmType);

			BizObj.AlgorithmType = OrgCodeAlgorithmType.Default;
			AssertEquals("AlgorithmType", OrgCodeAlgorithmType.Default, BizObj.AlgorithmType);

			BizObj.AlgorithmType = OrgCodeAlgorithmType.Override;
			AssertEquals("AlgorithmType", OrgCodeAlgorithmType.Override, BizObj.AlgorithmType);
		}

		public void TestCurrentOrgCodeLength()
		{
			BizObj.Elements[0].Order = 1;
			BizObj.Elements[1].Order = 2;
			BizObj.Elements[2].Order = 3;
			BizObj.Elements[3].Order = 4;

			BizObj.Elements[0].Length = 6;
			BizObj.Elements[1].Length = 7;
			BizObj.Elements[2].Length = 8;
			BizObj.Elements[3].Length = 9;

			AssertEquals("CurrentOrgCodeLength", (ZByte)30, BizObj.CurrentOrgCodeLength);
			AssertHasError(BizObj.CurrentOrgCodeLengthInfo, "The total length has exceeded the maximum length allowed for organization codes. Please reduce the length of one or more elements.");

			BizObj.Elements[1].Length = 6;
			BizObj.Elements[2].Length = 0;
			BizObj.Elements[3].Length = 0;
			AssertEquals("CurrentOrgCodeLength", (ZByte)12, BizObj.CurrentOrgCodeLength);
			AssertNoErrors(BizObj.CurrentOrgCodeLengthInfo);

			BizObj.Elements[0].Order = 0;
			BizObj.Elements[1].Order = 0;
			BizObj.Elements[2].Order = 0;
			BizObj.Elements[3].Order = 0;
			AssertEquals("CurrentOrgCodeLength", (ZByte)0, BizObj.CurrentOrgCodeLength);
			AssertHasError(BizObj.CurrentOrgCodeLengthInfo, "Organization codes cannot be empty, please select a non-zero order for one or more elements with non-zero length.");
		}

		public void TestElements()
		{
			OrgCodeElementCollectionTest.AssertElements(BizObj.Elements);
			AssertEquals("Elements.Parent", BizObj, BizObj.Elements.Parent);
			AssertEquals("Elements should be registered as an editable child object.", true, BizObj.IsRegisteredEditableChildObject(BizObj.Elements));
		}

		public void TestIOrgCodeAlgorithmElements()
		{
			var element1 = new OrgCodeElement();
			var element2 = new OrgCodeElement();
			BizObj.Elements.RemoveAll();
			BizObj.Elements.AddRange(element1, element2);

			var result = ((IOrgCodeAlgorithm)BizObj).Elements;
			AssertContainsExactElementsInAnyOrder(new[] { element1, element2 }, result);
		}

		public void TestIOrgCodeAlgorithmAllowRecalculatedOrgCodeByUser()
		{
			BizObj.AllowRecalculatedOrgCodeByUser = true;
			var result = ((IOrgCodeAlgorithm)BizObj).AllowRecalculatedOrgCodeByUser;
			AssertEquals(true, result);
		}

		public void TestIOrgCodeAlgorithmRegenerateOrgCodeOnChanges()
		{
			BizObj.RegenerateOrgCodeOnChanges = true;
			var result = ((IOrgCodeAlgorithm)BizObj).RegenerateOrgCodeOnChanges;
			AssertEquals(true, result);
		}

		public void TestIOrgCodeAlgorithmSelectableOrgTypes()
		{
			var element1 = new OrgCodeOrgType();
			var element2 = new OrgCodeOrgType();
			BizObj.SelectableOrgTypes.RemoveAll();
			BizObj.SelectableOrgTypes.AddRange(element1, element2);

			var result = ((IOrgCodeAlgorithm)BizObj).SelectableOrgTypes;
			AssertContainsExactElementsInAnyOrder(new[] { element1, element2 }, result);
		}

		public void TestMaxOrgCodeLength()
		{
			AssertEquals("MaxOrgCodeLength", (ZByte)OrgHeaderSchema.OH_Code.MaxLength, BizObj.MaxOrgCodeLength);
		}

		public void TestRunPreSaveValidation()
		{
			BizObj.Elements[0].Order = 1;
			BizObj.Elements[0].Length = 6;
			BizObj.CurrentOrgCodeLengthInfo.AddError("Error.");
			BizObj.RunPreSaveValidation();
			AssertNoErrors(BizObj);
		}

		public void TestSelectableOrgTypes()
		{
			OrgCodeOrgTypeCollectionTest.AssertElements(BizObj.SelectableOrgTypes);
			AssertEquals("SelectableOrgTypes should be registered as an editable child object.", true, BizObj.IsRegisteredEditableChildObject(BizObj.SelectableOrgTypes));
		}

		public void TestNeedsUNLOCO()
		{
			OrgCodeAlgorithm algo = GetTestAlgorithm();
			Assert("Precondition - all elements empty", !algo.NeedsUNLOCO());

			SetValues(algo, 1, 1, 1, 1, 1, 1, 0, 0);
			Assert("Does not need unloco still - these elements do not affect it", !algo.NeedsUNLOCO());

			SetValues(algo, 1, 1, 1, 1, 1, 1, 1, 0);
			Assert("needs unloco as unloco parameter is set", algo.NeedsUNLOCO());

			SetValues(algo, 1, 1, 1, 1, 1, 1, 0, 1);
			Assert("needs unloco as iata parameter is set", algo.NeedsUNLOCO());

			SetValues(algo, 1, 1, 1, 1, 1, 1, 1, 1);
			Assert("needs unloco as both parameters are set", algo.NeedsUNLOCO());

			SetValues(algo, 0, 0, 0, 0, 0, 0, 1, 1);
			Assert("needs unloco as both parameters are set", algo.NeedsUNLOCO());
		}

		static void SetValues(OrgCodeAlgorithm algo, byte el0, byte el1, byte el2, byte el3, byte el4, byte el5, byte el6, byte el7)
		{
			algo.Elements[OrgCodeElementDescription.CountryCode].Order = el0;
			algo.Elements[OrgCodeElementDescription.CountryCode].Length = el0;
			algo.Elements[OrgCodeElementDescription.CodeSpecificUniqueNumber].Order = el1;
			algo.Elements[OrgCodeElementDescription.CodeSpecificUniqueNumber].Length = el1;
			algo.Elements[OrgCodeElementDescription.FirstName].Order = el2;
			algo.Elements[OrgCodeElementDescription.FirstName].Length = el2;
			algo.Elements[OrgCodeElementDescription.GloballyUniqueNumber].Order = el3;
			algo.Elements[OrgCodeElementDescription.GloballyUniqueNumber].Length = el3;
			algo.Elements[OrgCodeElementDescription.LastName].Order = el4;
			algo.Elements[OrgCodeElementDescription.LastName].Length = el4;
			algo.Elements[OrgCodeElementDescription.SecondName].Order = el5;
			algo.Elements[OrgCodeElementDescription.SecondName].Length = el5;
			algo.Elements[OrgCodeElementDescription.UnlocoCode].Order = el6;
			algo.Elements[OrgCodeElementDescription.UnlocoCode].Length = el6;
			algo.Elements[OrgCodeElementDescription.IataCode].Order = el7;
			algo.Elements[OrgCodeElementDescription.IataCode].Length = el7;
		}
	}
}
