using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentWrappers
{
	public class DocGroup : DocumentWrapper
	{
		DocGroup(AccGroups accGroups, BusinessObjectFactory factoryToWrap)
			: base(accGroups, factoryToWrap)
		{
		}

		public static DocGroup New(AccGroups accGroups, BusinessObjectFactory factoryToWrap)
		{
			if (accGroups == null)
			{
				return null;
			}
			else
			{
				return new DocGroup(accGroups, factoryToWrap);
			}
		}

		AccGroups AccGroups
		{
			get { return (AccGroups)WrappedObject; }
		}

		public override string ToString()
		{
			return Description;
		}

		public ZString Code
		{
			get { return AccGroups.AR_Code; }
		}

		public ZString Description
		{
			get { return AccGroups.AR_DescMultilingual; }
		}
	}
}
