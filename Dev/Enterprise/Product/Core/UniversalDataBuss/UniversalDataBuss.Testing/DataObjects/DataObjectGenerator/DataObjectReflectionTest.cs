using System.Collections.Generic;
using NUnit.Framework;
using UniversalDataBuss.CodeGeneration;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal.Testing
{
	class DataObjectReflectionTest : TestCase
	{
		[RequiresSoftware(RequiredSoftware.VisualStudio)]
		[DatCapabilityRequirement("SOURCE_CODE")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1868:Unnecessary call to 'Contains(item)'", Justification = "Not used in a guarding capacity.")]
		public void TestCodeGeneratorEnforcesLazyCollectionSetters()
		{
			// The following classes still have public setters that are in the process of being made private.
			var exclusionList = new HashSet<string>
			{
				"MatchLine", "DocumentRequest", "InterchangeRequeueRequest", "CustomsSupportingInformation",
				"AdditionalContext", "AttachedDocument", "Workflow", "EntryInstruction", "InBondMoveDetail",
				"InBondMoveLineItem", "InBondMoveHeader", "TransactionBatch", "TransactionInfo", "PostingJournal",
				"Context", "Event", "AdditionalBill", "HazardousMaterial", "CustomsReference", "AddInfoGroup",
				"CommercialInfo", "CommercialInvoiceHeader", "CommercialInvoiceLine", "Container", "EntryHeader", "EntryLine"
			};

			var classesNeedingAutoCodeGeneration = new List<string>();

			DataObjectGenerator.InterceptForTesting = (bool hasChanges, string className, string filePath, string generatedText) =>
			{
				if (!hasChanges && FileContentHelper.FileContentsDiffer(filePath, generatedText))
				{
					hasChanges = true;
				}

				if (hasChanges)
				{
					if (exclusionList.Contains(className))
					{
						exclusionList.Remove(className);    // Previously excluded, but no longer needs to be.
					}
					else
					{
						classesNeedingAutoCodeGeneration.Add(className);    // Needs generation but wasn't excluded.
					}
				}
			};

			try
			{
				DataObjectGenerator.Generate(BaseSourcePath);
			}
			finally
			{
				DataObjectGenerator.InterceptForTesting = null;
			}

			AssertNoStaleExclusions(exclusionList);
			AssertNoMissingLazySetterGeneration(classesNeedingAutoCodeGeneration);

			Assert("Code generation validation should pass with no issues.", classesNeedingAutoCodeGeneration.Count == 0 && exclusionList.Count == 0);
		}

		void AssertNoStaleExclusions(HashSet<string> exclusions)
		{
			if (exclusions == null || exclusions.Count == 0)
			{
				return;
			}

			var message = "The following classes were included in the exclusion list but were unchanged by the code generator. These classes should be removed:\r\n" +
				string.Join(", ", exclusions);

			Fail(message);
		}

		void AssertNoMissingLazySetterGeneration(List<string> classes)
		{
			if (classes == null || classes.Count == 0)
			{
				return;
			}

			var message = "The following classes are missing code-generated lazy setters for collection properties.\r\n" +
				"Run the generator via '..\\Dev\\Bin\\UniversalDataBuss.CodeGeneration.exe' or the project directly.\r\n" +
				"Classes: " + string.Join(", ", classes);

			Fail(message);
		}
	}
}
