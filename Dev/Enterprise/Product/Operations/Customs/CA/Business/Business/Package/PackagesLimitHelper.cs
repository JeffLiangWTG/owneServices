using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public static class PackagesLimitHelper
	{
		public static void HookCheckPackUnitCountEvent(BaseCusLinkPackage packageLink, ICusLinkPackageSupporter supporter, BaseCusLinkPackageCollection collection)
		{
			if (packageLink != null && packageLink.Package != null)
			{
				packageLink.Package.CW_PackTypeInfo.ValueChanged -= CheckPackageUnitCountEventHandler(supporter, collection);
				packageLink.Package.CW_PackTypeInfo.ValueChanged += CheckPackageUnitCountEventHandler(supporter, collection);
				packageLink.IsLinkedInfo.ValueChanged -= CheckPackageUnitCountEventHandler(supporter, collection);
				packageLink.IsLinkedInfo.ValueChanged += CheckPackageUnitCountEventHandler(supporter, collection);
			}
		}

		public static void CheckPackageUnitCount(ICusLinkPackageSupporter supporter, BaseCusLinkPackageCollection collection)
		{
			foreach (BaseCusLinkPackage cusLinkPackage in collection)
			{
				using (((ISingleElementListInternal)cusLinkPackage).SuspendListChanged())
				{
					if (cusLinkPackage != null && cusLinkPackage.Package != null)
					{
						cusLinkPackage.RemoveRowMessageError(LimitMessageError);
						if (cusLinkPackage.IsLinked && supporter.DistinctPackageTypes.Count > DifferentUnitLimit)
						{
							cusLinkPackage.AddRowMessageError(LimitMessageError);
						}
					}
				}
			}
		}

		public static EventHandler CheckPackageUnitCountEventHandler(ICusLinkPackageSupporter supporter, BaseCusLinkPackageCollection collection) => delegate { CheckPackageUnitCount(supporter, collection); };

		static ZInt DifferentUnitLimit => 9;

		static ZString LimitMessageError => Res.GetString("95F75204-59E0-408E-88A3-7425D8FEA799", "No more than {0} different units of measure may be used here. Note that pack lines with marks and numbers cannot be combined.", DifferentUnitLimit);
	}
}
