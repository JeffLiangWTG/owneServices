using System;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class AQISPackageCollection : AQISCollection<AQISPackage>
	{
		public AQISPackageCollection(BusinessObjectFactory factory, AUAddInfo addInfo)
			: base(factory, addInfo)
		{
		}

		protected override BusinessObject BusinessObjectToAddToCollection(ZString value1, ZString value2)
		{
			AQISPackage package = new AQISPackage(Factory);
			package.Number = Convert.ToInt32(value1);
			package.Type = value2;

			return package;
		}

		public override void ReBuildAndSaveAQISElements()
		{
			ZStringBuilder result = new ZStringBuilder();

			foreach (AQISPackage package in this)
			{
				result.Append(package.Number.ToString());
				result.Append("/");
				result.Append(package.Type);
				result.Append(",");
			}

			AddInfo.ZA_AQISPackageType_Hidden = new ZString(result.ToString()).TrimEndIncludingWhiteSpace(',');
		}

		public AQISPackage FindAQISPackageByType(string type)
		{
			AQISPackage result = null;

			foreach (AQISPackage package in this)
			{
				if (package.Type == type)
				{
					result = package;
					break;
				}
			}

			return result;
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new AQISPackage(Factory);
		}
	}
}
