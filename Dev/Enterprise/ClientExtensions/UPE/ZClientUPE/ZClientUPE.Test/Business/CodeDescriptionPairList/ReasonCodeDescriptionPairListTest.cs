using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.UPE.Business.Testing
{
	public class ReasonCodeDescriptionPairListTest : TestCaseWithFactory
	{
		public void TestGetReasonList()
		{
			AssertQueueInQueueList(new CommercialQueueCodeDescriptionPairList());
			AssertQueueInQueueList(new CargoReportQueueCodeDescriptionPairList());
			AssertQueueInQueueList(new DeclarationQueueCodeDescriptionPairList());
		}

		public void TestGetReasonListForCommercialQueueList()
		{
			TestGetReasonListForWholeList(new CommercialQueueCodeDescriptionPairList());
		}

		public void TestGetReasonListForCargoReportQueueList()
		{
			TestGetReasonListForWholeList(new CargoReportQueueCodeDescriptionPairList());
		}

		public void TestGetReasonListForDeclarationQueueList()
		{
			TestGetReasonListForWholeList(new DeclarationQueueCodeDescriptionPairList());
		}

		public void TestGetReasonList_WhenQueueCodeDoesntHaveAStatusSubList()
		{
			AssertEquals("When the Queue code doesnt have a reason list, an empty list should be returned", 0, ReasonCodeDescriptionPairList.GetReasonList(new DefaultQueueCodeDescriptionPairList(), "XXXX").Count);
		}

		public void TestReasonCodeSubListNestedTypeName_HasToBeAValidQueueCode()
		{
			Type[] reasonSubListTypes = typeof(AutoReasonCodeDescriptionPairList).GetNestedTypes();
			DeclarationQueueCodeDescriptionPairList declarationQueueList = new DeclarationQueueCodeDescriptionPairList();
			CargoReportQueueCodeDescriptionPairList cargoReportQueueList = new CargoReportQueueCodeDescriptionPairList();
			CommercialQueueCodeDescriptionPairList commercialQueueList = new CommercialQueueCodeDescriptionPairList();
			AssertEquals("There should be reason sub lists", true, reasonSubListTypes.Length > 0);
			foreach (Type reasonSubListType in reasonSubListTypes)
			{
				if (reasonSubListType.Name != "Codes" && reasonSubListType.Name != "Descriptions")
				{
					ZString reasonSubListPrefix = reasonSubListType.Name[0].ToString();
					ZString queueName = ((ZString)reasonSubListType.Name).SubstringSafe(1);
					if (queueName.Length == 3 && reasonSubListPrefix.ContainsAnyChar("ACD"))
					{
						DefaultQueueCodeDescriptionPairList listToCheck;
						if (reasonSubListPrefix == "A")
						{
							listToCheck = cargoReportQueueList;
						}
						else if (reasonSubListPrefix == "C")
						{
							listToCheck = commercialQueueList;
						}
						else
						{
							listToCheck = declarationQueueList;
						}

						Assert(queueName + " should be part of the Queue Code List", listToCheck.ContainsCode(queueName));
					}
					else
					{
						Fail("Invalid Reason Sub-List Type name format");
					}
				}
			}
		}

		void AssertQueueInQueueList(DefaultQueueCodeDescriptionPairList queueList)
		{
			string prefix;
			if (queueList is CommercialQueueCodeDescriptionPairList)
			{
				prefix = "C";
			}
			else if (queueList is CargoReportQueueCodeDescriptionPairList)
			{
				prefix = "A";
			}
			else
			{
				prefix = "D";
			}

			foreach (CodeDescriptionPair queue in queueList)
			{
				CodeDescriptionPairList reasonList = ReasonCodeDescriptionPairList.GetReasonList(queueList, queue.Code);
				Type reasonSubListType = typeof(AutoReasonCodeDescriptionPairList).GetNestedType(prefix + queue.Code);
				if (reasonSubListType != null)
				{
					AssertEquals("Reason list specified for Queue code '" + queue.Code + "' but no reasons were returned from GetReasonList", true, reasonList.Count > 0);
				}
			}
		}

		void TestGetReasonListForWholeList(DefaultQueueCodeDescriptionPairList queueList)
		{
			ReasonCodeDescriptionPairList generatedList = ReasonCodeDescriptionPairList.GetReasonList(queueList);
			CodeDescriptionPairList expectedList = new CodeDescriptionPairList();
			foreach (CodeDescriptionPair queue in queueList)
			{
				expectedList.AddRange(ReasonCodeDescriptionPairList.GetReasonList(queueList, queue.Code));
			}

			Assert("There has to be reason codes", generatedList.Count > 0);
			AssertEquals("Count has to be equal", expectedList.Count, generatedList.Count);
		}
	}
}
