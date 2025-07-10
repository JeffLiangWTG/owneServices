#if DEBUG
using System.Collections;
using System.Collections.Generic;
using System.Data;
using CargoWise.Types;
using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing
{
	public class LightValidationTester
	{
		public LightValidationTester(BusinessObject bo)
		{
			LightValidationBO = bo;
		}

		public void Test()
		{
			Initialise();

			if (LightValidationBO.LightValidationEnabled)
			{
				CalculatePropertiesAccessedByPreSaveValidation();

				foreach (ZPropertyInfo info in GetPropertiesToTest())
				{
					long markInvalidCount = LightValidationBO.ManualCallsToMarkAsNeedingValidation_CountForTesting;
					LightValidationBO.MarkLightValidationAsValidForTesting();
					IZType originalValue = info.Value;
					ZPropertyInfoTestHelper.SetValue(info);
					info.Value = originalValue;

					if (!LightValidationBO.IsDeleted && LightValidationBO.ManualCallsToMarkAsNeedingValidation_CountForTesting == markInvalidCount) // not marked as needing validation
					{
						errors.Append("Setting property '" + info.Name + "' on '" + info.BizObj.GetType().FullName + "' should call MarkAsNeedingValidation on '" + LightValidationBO.GetType().FullName + "'");
					}
				}

				if (errors.Length > 0)
				{
					TestCase.Fail("\r\n\r\n" + errors.ToStringWithNewLineBetweenAppends() + "\r\n\r\n");
				}
			}
			else
			{
				TestCase.Assert("List validation is disabled so this test is not relevant", true);
			}
		}
		ZStringBuilder errors;

		void Initialise()
		{
			accessedColumnsOnOtherObjects = new Dictionary<string, BusinessObject>();
			accessedOtherObjects = new Dictionary<BusinessObject, bool>();

			errors = new ZStringBuilder();
		}

		void CalculatePropertiesAccessedByPreSaveValidation()
		{
			LightValidationBO.MarkAsNeedingValidation();

			LightValidationBO.Factory.AccessingPersistentValueForTesting += new BusinessObjectFactory.AccessingPersistentValueForTestingDelegate(bo_AccessingPersistentValueForTesting);

			LightValidationBO.RunPreSaveValidationInternal(false);

			LightValidationBO.Factory.AccessingPersistentValueForTesting -= new BusinessObjectFactory.AccessingPersistentValueForTestingDelegate(bo_AccessingPersistentValueForTesting);
		}

		void bo_AccessingPersistentValueForTesting(BusinessObject bo, DataColumn column)
		{
			if (bo != LightValidationBO)
			{
				accessedColumnsOnOtherObjects[column.ColumnName + "_" + bo.GetType().FullName] = bo;
				accessedOtherObjects[bo] = true;
			}
		}

		protected virtual bool ShouldTestProperty(ZPropertyInfo info)
		{
			return true;
		}

		IEnumerable<ZPropertyInfo> GetPropertiesToTest()
		{
			Dictionary<string, ZPropertyInfo> hash = new Dictionary<string, ZPropertyInfo>();
			foreach (BusinessObject b in GetBusinessObjectsToTest())
			{
				foreach (ZPropertyInfo info in b.ZPropertyInfoHash)
				{
					if (ShouldTestProperty(info))
					{
						string key = info.Name + "_" + info.BizObj.GetType().FullName;
						if (info.IsPersistent &&
							accessedColumnsOnOtherObjects.ContainsKey(key) &&
							accessedColumnsOnOtherObjects[key] == info.BizObj &&
							!info.Name.EndsWith("IsActive") && !info.Name.EndsWith("IsCancelled") &&
							info.PropertyDescriptor.Attributes[typeof(LightValidationTestExempt)] == null)
						{
							hash[info.BizObj.GetType().FullName + info.Name] = info;
						}
					}
				}
			}
			List<ZPropertyInfo> list = new List<ZPropertyInfo>(hash.Values);
			list.Sort(delegate(ZPropertyInfo info1, ZPropertyInfo info2)
			{ return info1.BizObj.GetType().Name.CompareTo(info2.BizObj.GetType().Name); });
			return list;
		}

		IEnumerable<BusinessObject> GetBusinessObjectsToTest()
		{
			Hashtable hash = new Hashtable();
			foreach (BusinessObject bo in accessedOtherObjects.Keys)
			{
				if (!hash.ContainsKey(bo) &&
						(HasDescendent(bo, LightValidationBO) || HasDescendent(LightValidationBO, bo))) // anywhere in registered editable tree
				{
					AddEntityAndDescendents(hash, bo);
				}
			}
			return GetBusinessObjectsFromKeys(hash);
		}

		IEnumerable<BusinessObject> GetBusinessObjectsFromKeys(Hashtable hash)
		{
			foreach (IBusiness entity in hash.Keys)
			{
				if (entity is BusinessObject)
				{
					yield return (BusinessObject)entity;
				}
			}
		}

		void AddEntityAndDescendents(Hashtable hash, IBusiness entity)
		{
			if (!hash.ContainsKey(entity))
			{
				hash[entity] = true;
				foreach (IBusiness child in entity.Children)
				{
					AddEntityAndDescendents(hash, child);
				}
			}
		}

		bool HasDescendent(IBusiness potentialParent, IBusiness child)
		{
			if (HasChild(potentialParent, child))
			{
				return true;
			}

			foreach (IBusiness b in potentialParent.Children)
			{
				if (HasDescendent(b, child))
				{
					return true;
				}
			}

			return false;
		}

		bool HasChild(IBusiness potentialParent, IBusiness child)
		{
			foreach (IBusiness b in potentialParent.Children)
			{
				if (b == child)
				{
					return true;
				}
			}
			return false;
		}

		Dictionary<string, BusinessObject> accessedColumnsOnOtherObjects;
		Dictionary<BusinessObject, bool> accessedOtherObjects;

		readonly BusinessObject LightValidationBO;
	}
}

#endif
