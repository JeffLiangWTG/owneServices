using System;
using CargoWise.Integration;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Res = Enterprise.ZArchitecture.Core.SourceGenerated.Res;

namespace Enterprise.ZArchitecture.Environment
{
	public class CodePairRegistryDataType : StringRegistryDataType
	{
		public CodePairRegistryDataType(OLookUpEditType lookUpEditType)
		{
			LookUpEditType = lookUpEditType;
		}

		public CodePairRegistryDataType(OLookUpEditType lookUpEditType, bool allowBlank, bool validateCode)
		{
			LookUpEditType = lookUpEditType;
			AllowBlank = allowBlank;
			ValidateCode = validateCode;
		}

		public CodePairRegistryDataType(ICodeDescriptionPairListProvider lookUpListProvider, bool allowBlank, bool validateCode)
		{
			this.lookUpListProvider = lookUpListProvider;
			AllowBlank = allowBlank;
			ValidateCode = validateCode;
		}

		public OLookUpEditType LookUpEditType
		{
			get { return lookUpEditType; }
			set { lookUpEditType = value; }
		}

		public virtual CodeDescriptionPairList LookUpList
		{
			get
			{
				return LookUpListProvider.CodeDescriptionPairList;
			}
		}

		protected ICodeDescriptionPairListProvider LookUpListProvider
		{
			get
			{
				if (lookUpListProvider == null)
				{
					lookUpListProvider = new CodeDescriptionPairListProvider(() => new CodeDescriptionPairList(lookUpEditType));
				}

				return lookUpListProvider;
			}
		}
		ICodeDescriptionPairListProvider lookUpListProvider;

		public bool AllowBlank
		{
			get { return allowBlank; }
			set { allowBlank = value; }
		}

		public bool ValidateCode
		{
			get { return validateCode; }
			set { validateCode = value; }
		}

		protected override IRegistryEditorInfo GetNewDefaultEditorInfo()
		{
			return new ComboBoxRegistryEditorInfo(LookUpListProvider);
		}

		protected override void ValidateCore(IRegistryItem registryItem, string proposedValue, Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			if (ValidateCode)
			{
				if (AllowBlank && proposedValue.Trim().Length == 0)
				{
					return;
				}

				foreach (ICodeDescription pair in LookUpList)
				{
					if (proposedValue == pair.Code)
					{
						return;
					}
				}

				throw new RegistryValidationException(Res.GetString("e9acd160-5292-49e4-803c-12f28b8fe49c", "Invalid Selection. Please choose a code from the list."));
			}
		}

		OLookUpEditType lookUpEditType;
		bool allowBlank;
		bool validateCode = true;
	}
}
