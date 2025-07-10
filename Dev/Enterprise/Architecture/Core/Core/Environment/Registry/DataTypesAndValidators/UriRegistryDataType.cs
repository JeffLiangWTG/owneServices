using System;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.Environment
{
	public class UriRegistryDataType : StringRegistryDataType
	{
		public UriRegistryDataType(string uriScheme)
			: base()
		{
			InitializeUriScheme(uriScheme);

			AllowAutoProtocolPrefixing = true;
			AllowQueryString = true;
		}

		public UriRegistryDataType(string uriScheme, int minLength, int maxLength)
			: base(minLength, maxLength)
		{
			InitializeUriScheme(uriScheme);

			AllowAutoProtocolPrefixing = true;
		}

		public bool AllowAutoProtocolPrefixing { get; set; }

		public bool AllowQueryString { get; set; }

		void InitializeUriScheme(string uriScheme)
		{
			if (string.IsNullOrEmpty(uriScheme))
			{
				throw new ArgumentNullException(uriScheme);
			}

			this.uriScheme = uriScheme;
		}
		string uriScheme;

		protected override void ValidateCore(IRegistryItem registryItem, string proposedValue, Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			base.ValidateCore(registryItem, proposedValue, companyPK, branchPK, departmentPK);
			UriRegistryTypeValidator.ValidateUri(proposedValue, uriScheme, AllowAutoProtocolPrefixing, AllowQueryString);
		}
	}
}
