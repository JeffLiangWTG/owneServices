using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.ES.NCTS.Business
{
	public class UnitCountRelatedToUnitTypeChecker
	{
		public UnitCountRelatedToUnitTypeChecker(ICheckablePackage currentPackage)
		{
			this.currentPackage = Argument.NotNull(currentPackage, nameof(currentPackage));
			unitCountInfo = Argument.NotNull(currentPackage.UnitCountInfo, nameof(UnitCountRelatedToUnitTypeChecker.currentPackage.UnitCountInfo));
		}

		readonly ICheckablePackage currentPackage;
		readonly ZPropertyInfo unitCountInfo;

		protected virtual ZString QuantityCanBeZeroOnlyWhenTypeAndMarkAreTheSame => Res.GetString("EC1BE737-1247-47A3-BCFF-8495D3F96892", "Quantity can be zero only if 'Type' and 'Marks and Numbers' are the same as those of the other line item.");

		public void Check()
		{
			var packagesCollection = currentPackage.GetRelatedEntryPackages();

			if (packagesCollection.Any())
			{
				CheckHavePackageWithSameTypeAndMarksAndUnitCountNoZero(packagesCollection);
			}
			else
			{
				AddMessageErrorToUnitCount(unitCountInfo);
			}
		}

		void CheckHavePackageWithSameTypeAndMarksAndUnitCountNoZero(IEnumerable<ICheckablePackage> packagesCollection)
		{
			if (currentPackage.UnitCount == 0)
			{
				var exist = 0;
				foreach (var package in packagesCollection)
				{
					if (package.UnitType == currentPackage.UnitType && package.MarksAndNumbers == currentPackage.MarksAndNumbers && package.UnitCount != 0)
					{
						exist++;
						break;
					}
				}
				if (exist == 0)
				{
					AddMessageErrorToUnitCount(unitCountInfo);
				}
				else
				{
					unitCountInfo.ClearValue();
				}
			}
		}

		void AddMessageErrorToUnitCount(ZPropertyInfo unitCountInfo) => unitCountInfo.AddMessageError(QuantityCanBeZeroOnlyWhenTypeAndMarkAreTheSame);
	}
}
