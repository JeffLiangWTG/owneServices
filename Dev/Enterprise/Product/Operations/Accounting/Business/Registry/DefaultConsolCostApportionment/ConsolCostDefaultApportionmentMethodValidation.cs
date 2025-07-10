using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesConstants;
using ResString = Enterprise.Accounting.Business.ResString;

namespace Enterprise.Accounting.Registry.Business
{
	public class ConsolCostDefaultApportionmentMethodValidation
	{
		public ConsolCostDefaultApportionmentMethodValidation(ConsolCostDefaultApportionmentMethod parent)
		{
			this.Parent = parent;
		}

		readonly ConsolCostDefaultApportionmentMethod Parent;

		public void ValidateModule()
		{
			MandatoryValidation.CheckEntered(Parent.ModuleInfo, (IMultilingualString)ResString.GetMultilingualString("19b068c2-abec-44bb-b189-8f7d8de12b49", "Module"));
			ListValidation.ErrorIfInvalidCode(Parent.ModuleInfo, Parent.Lookups.ModuleList);
			ValidateMethodCollisions();
		}

		public void ValidateTransportMode()
		{
			MandatoryValidation.CheckEntered(Parent.TransportModeInfo, (IMultilingualString)ResString.GetMultilingualString("b591a808-1f40-4813-b170-1c5d576a48ea", "Transport Mode"));
			ListValidation.ErrorIfInvalidCode(Parent.TransportModeInfo, Parent.Lookups.TransportModeList);
			ValidateMethodCollisions();
		}

		public void ValidateContainerMode()
		{
			MandatoryValidation.CheckEntered(Parent.ContainerModeInfo, (IMultilingualString)ResString.GetMultilingualString("2cce6caf-6387-4279-ac74-2d410e8d3e13", "Container Mode"));
			ListValidation.ErrorIfInvalidCode(Parent.ContainerModeInfo, Parent.Lookups.ContainerModeList);
			ValidateMethodCollisions();
		}

		public void ValidateConsolType()
		{
			MandatoryValidation.CheckEntered(Parent.ConsolTypeInfo, (IMultilingualString)ResString.GetMultilingualString("846f2eab-dbfa-44a2-b355-84f186426a85", "Consol Type"));
			ListValidation.ErrorIfInvalidCode(Parent.ConsolTypeInfo, Parent.Lookups.ConsolTypeList);
			ValidateMethodCollisions();
		}

		public void ValidateApportionment()
		{
			ValidateApportionmentList();
			ValidateMethodCollisions();
		}

		public void ValidateApportionmentList()
		{
			MandatoryValidation.CheckEntered(Parent.ApportionmentInfo, (IMultilingualString)ResString.GetMultilingualString("976fbad9-d052-45a7-b606-cb0d26d2fb3a", "Apportionment"));
			ListValidation.ErrorIfInvalidCode(Parent.ApportionmentInfo, Parent.Lookups.ApportionmentList);
		}

		public void ValidateDirection()
		{
			MandatoryValidation.CheckEntered(Parent.DirectionInfo, (IMultilingualString)ResString.GetMultilingualString("2B6EB691-FC2C-46A9-A407-97E2FEDD363C", "Direction"));
			ListValidation.ErrorIfInvalidCode(Parent.DirectionInfo, Parent.Lookups.DirectionList);
			ValidateMethodCollisions();
		}

		void ValidateMethodCollisions()
		{
			if (Parent.ParentCollection == null)
			{
				return;
			}
			var duplicateMessage = ResString.GetMultilingualString("6868817b-d80a-4a9e-873c-7fc121bd7eae", "Duplicated apportionment methods");
			var methodConflictMessage = ResString.GetMultilingualString("9e90dca7-8a40-409e-8e1e-5991f31fd5e4", "Other conflicting apportionment method rules exist");
			var methodConflict = false;
			var duplicate = false;
			foreach (ConsolCostDefaultApportionmentMethod apportionmentMethod in Parent.ParentCollection)
			{
				if (IsMatchingMethods(apportionmentMethod, Parent, false)
				&& apportionmentMethod.Apportionment != Parent.Apportionment)
				{
					methodConflict = true;
					apportionmentMethod.ApportionmentInfo.AddError(methodConflictMessage);
					Parent.ClearRowNotifications();
				}
				else if (IsMatchingMethods(apportionmentMethod, Parent, true)
				&& apportionmentMethod.Apportionment == Parent.Apportionment
				&& apportionmentMethod.PK != Parent.PK)
				{
					duplicate = true;
					apportionmentMethod.AddRowError(duplicateMessage);
				}

				if (apportionmentMethod.ParentCollection == null)
				{
					apportionmentMethod.ParentCollection = Parent.ParentCollection;
					apportionmentMethod.Validation.ValidateMethodCollisions();
				}
			}

			if (methodConflict)
			{
				Parent.ApportionmentInfo.AddError(methodConflictMessage);
			}
			else
			{
				Parent.ValidateApportionmentList();
			}

			if (duplicate)
			{
				Parent.AddRowError(duplicateMessage);
			}
			else
			{
				Parent.ClearRowNotifications();
			}
		}

		bool IsMatchingMethods(ConsolCostDefaultApportionmentMethod search, ConsolCostDefaultApportionmentMethod original, bool consideringAllEqual)
		{
			return IsEqualMethodProperty(search.ContainerMode, original.ContainerMode, consideringAllEqual)
				&& IsEqualMethodProperty(search.TransportMode, original.TransportMode, consideringAllEqual)
				&& IsEqualMethodProperty(search.ConsolType, original.ConsolType, consideringAllEqual)
				&& IsEqualMethodProperty(search.Module, original.Module, consideringAllEqual)
				&& IsEqualMethodProperty(search.Direction, original.Direction, consideringAllEqual);
		}

		bool IsEqualMethodProperty(string alpha, string bravo, bool consideringAll)
		{
			return alpha == bravo || (consideringAll && (IsAllString(alpha) || IsAllString(bravo)));
		}

		bool IsAllString(string input)
		{
			return input == ApportionmentMethod.AllCode;
		}
	}
}
