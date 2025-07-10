using System;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class PackageCollection : Customs.Business.BasePackageCollection
	{
		public PackageCollection(PackingGroup multiPackingGroup)
			: base(multiPackingGroup)
		{
		}

		public bool HasAtLeastOneOtherElementBesidesThisOne(Package package)
		{
			foreach (Package p in this)
			{
				if (p != package)
				{
					return true;
				}
			}
			return false;
		}

		public new Package this[int index]
		{
			get { return (Package)base[index]; }
		}

		public new Package AddNew()
		{
			return (Package)base.AddNew();
		}

		public override Type GetTypeOfElementsFromPK(ZGuid pK)
		{
			return typeof(Package);
		}

		public Package AddNew(ZString packType)
		{
			Package result = AddNew();
			result.CW_PackType = packType;
			return result;
		}

		public Package GetElementWithPackType(ZString packType)
		{
			foreach (Package package in this)
			{
				if (package.CW_PackType == packType)
				{
					return package;
				}
			}
			return null;
		}
	}
}
