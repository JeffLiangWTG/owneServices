using System;
using System.Text;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Res = Enterprise.ZArchitecture.Core.SourceGenerated.Res;

namespace Enterprise.ZArchitecture.Environment
{
	public class ServiceUrlRegistryDataType : RegistryDataType<string>
	{
		public ServiceUrlRegistryDataType() : base(RegistryDataTypes.StringType.Code, string.Empty)
		{
		}

		protected override byte[] SerialiseCore(string value)
		{
			return Encoding.Unicode.GetBytes(value ?? StringRegistryDataType.MagicNullString);
		}

		protected override string DeserialiseCore(byte[] value)
		{
			string asString = Encoding.Unicode.GetString(value);
			return (asString == StringRegistryDataType.MagicNullString) ? string.Empty : asString;
		}

		protected override bool ValuesAreEqualCore(string a, string b)
		{
			return Equals(a, b);
		}

		protected override bool AllowNullCore
		{
			get { return false; }
		}

		protected override IRegistryEditorInfo GetNewDefaultEditorInfo()
		{
			return new ServiceUrlRegistryEditorInfo();
		}

		protected override void ValidateCore(IRegistryItem registryItem, string proposedValue, Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			if (!string.IsNullOrEmpty(proposedValue))
			{
				if (!UrlValidation.IsValidAbsoluteHttpOrHttpsUrl(proposedValue))
				{
					throw new RegistryValidationException(Res.GetString("36371888-a965-40ec-8741-be2ed9efc649", "The URL is invalid, please input a valid URL that must be HTTPS or HTTP."));
				}
			}
		}

		public override bool IsDefaultValueImmutable => true;
	}
}
