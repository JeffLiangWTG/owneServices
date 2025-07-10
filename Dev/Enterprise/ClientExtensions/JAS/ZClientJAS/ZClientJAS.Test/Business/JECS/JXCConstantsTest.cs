using System;
using System.Collections;
using System.Reflection;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Client.JAS.Business.JXC
{
	public class JXCConstantsTest : TestCase
	{
		public void TestEveryFieldPositionsClassHasValidFieldCountAndUniqueFieldPositions()
		{
			BindingFlags bindingFlag = (BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
			Type[] nestedTypes = typeof(JXCConstants).GetNestedTypes(bindingFlag);
			foreach (Type nestedType in nestedTypes)
			{
				if (nestedType.Name.EndsWith("FieldPositions") && nestedType.Name != "FieldPositions" && !IsExcluded(nestedType))
				{
					string fieldCountConstName = ((ZString)nestedType.Name).Left(nestedType.Name.Length - 14) + "FieldCount";
					FieldInfo fieldInfo = typeof(JXCConstants).GetField(fieldCountConstName, bindingFlag);
					if (fieldInfo == null)
					{
						Fail(fieldCountConstName + " constant does not exist. This is needed to determine the number of fields expected for the LineType");
					}
					else
					{
						int fieldCount = (int)fieldInfo.GetValue(null);
						FieldInfo[] positionFieldInfos = nestedType.GetFields(BindingFlags.Public | BindingFlags.Static);
						PropertyInfo[] positionPropertyInfos = nestedType.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty);
						ArrayList fieldPositions = new ArrayList(positionFieldInfos.Length + positionPropertyInfos.Length);
						foreach (FieldInfo positionFieldInfo in positionFieldInfos)
						{
							int fieldPosition = (int)positionFieldInfo.GetValue(null);
							AssertFieldPosition(positionFieldInfo.Name, fieldCount, fieldPosition, fieldPositions);
						}

						foreach (PropertyInfo positionPropertyInfo in positionPropertyInfos)
						{
							object fieldPositionsInstance = Activator.CreateInstance(nestedType);
							int fieldPosition = (int)positionPropertyInfo.GetValue(fieldPositionsInstance, null);
							AssertFieldPosition(nestedType.Name + "." + positionPropertyInfo.Name, fieldCount, fieldPosition, fieldPositions);
						}
					}
				}
			}
		}

		public void TestGetMessageCategoryFromMessageType()
		{
			AssertMessageCategoryFromMessageType(JXCConstants.MessageCategories.Air, JXCConstants.MessageTypes.CHAB);
			AssertMessageCategoryFromMessageType(JXCConstants.MessageCategories.Air, JXCConstants.MessageTypes.MAWB);
			AssertMessageCategoryFromMessageType(JXCConstants.MessageCategories.Air, JXCConstants.MessageTypes.DAWB);
			AssertMessageCategoryFromMessageType(JXCConstants.MessageCategories.Air, JXCConstants.MessageTypes.PSAB);
			AssertMessageCategoryFromMessageType(JXCConstants.MessageCategories.Ocean, JXCConstants.MessageTypes.OMAN);
			AssertMessageCategoryFromMessageType(JXCConstants.MessageCategories.Ocean, JXCConstants.MessageTypes.COHB);
			AssertMessageCategoryFromMessageType(JXCConstants.MessageCategories.Ocean, JXCConstants.MessageTypes.PSBL);
			AssertMessageCategoryFromMessageType(JXCConstants.MessageCategories.Financial, JXCConstants.MessageTypes.AINV);
			AssertMessageCategoryFromMessageType(JXCConstants.MessageCategories.Financial, JXCConstants.MessageTypes.ACDT);
			AssertMessageCategoryFromMessageType(JXCConstants.MessageCategories.Financial, JXCConstants.MessageTypes.MINV);
			AssertMessageCategoryFromMessageType(JXCConstants.MessageCategories.Financial, JXCConstants.MessageTypes.MCDT);
			AssertMessageCategoryFromMessageType(JXCConstants.MessageCategories.Financial, JXCConstants.MessageTypes.NINV);
			AssertMessageCategoryFromMessageType(JXCConstants.MessageCategories.Financial, JXCConstants.MessageTypes.NCDT);
			AssertMessageCategoryFromMessageType(JXCConstants.MessageCategories.Financial, JXCConstants.MessageTypes.ISMY);
			AssertMessageCategoryFromMessageType(JXCConstants.MessageCategories.Financial, JXCConstants.MessageTypes.APRS);
			AssertMessageCategoryFromMessageType(JXCConstants.MessageCategories.Miscellaneous, JXCConstants.MessageTypes.LINK);
			AssertMessageCategoryFromMessageType(JXCConstants.MessageCategories.Miscellaneous, JXCConstants.MessageTypes.GSUM);
			AssertMessageCategoryFromMessageType(JXCConstants.MessageCategories.Miscellaneous, JXCConstants.MessageTypes.CCCV);
			AssertMessageCategoryFromMessageType(JXCConstants.MessageCategories.Unknown, "USUCK");
		}

		bool IsExcluded(Type nestedType)
		{
			return nestedType == typeof(JXCConstants.AWBFieldPositions) || nestedType == typeof(JXCConstants.INVCDTFieldPositions);
		}

		void AssertFieldPosition(string fieldName, int fieldCount, int fieldPosition, ArrayList positionList)
		{
			string invalidIndexMsg = "Field index/position has to be less than the specified field count. ({0})";
			Assert(string.Format(invalidIndexMsg, fieldName), fieldCount > fieldPosition);
			string nonUniqueIndexMsg = "Field Position index has to be unique. ({0})";
			Assert(string.Format(nonUniqueIndexMsg, fieldName), !positionList.Contains(fieldPosition));
			positionList.Add(fieldPosition);
		}

		void AssertMessageCategoryFromMessageType(JXCConstants.MessageCategories expectedCategory, ZString messageType)
		{
			AssertEquals("Incorrect category", expectedCategory, JXCConstants.MessageTypes.GetMessageCategoryFromMessageType(messageType));
		}
	}
}
