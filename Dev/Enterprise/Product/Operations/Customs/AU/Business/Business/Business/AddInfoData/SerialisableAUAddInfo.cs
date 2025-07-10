using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public abstract class SerialisableAUAddInfo : AutoAUAddInfo
	{
		protected SerialisableAUAddInfo(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public new static class Schema
		{
			public const string TablePrefix = AUAddInfoSchema.Constants.Prefix + "_";
		}

		public override string TablePrefix
		{
			get { return Schema.TablePrefix; }
		}

		public const char SeperationCharacter = '*';

		protected override int GetDefaultAccessedZInfoPropertiesCapacity()
		{
			return ZPropertyInfoHash.Count;
		}

		public override string ToString()
		{
			string result = "";
			foreach (ZPropertyInfo propertyInfo in ZPropertyInfoHash)
			{
				if (propertyInfo.HasSetter && propertyInfo.Name.Substring(0, 3) == TablePrefix)
				{
					IZType value = propertyInfo.Value;

					if (!value.IsDefault)
					{
						if (result.Length > 0)
						{
							result += SeperationCharacter;
						}

						result += propertyInfo.Name.Substring(3) + "=" + value.ToString();
					}
				}
			}
			return result;
		}
	}
}
