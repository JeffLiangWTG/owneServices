using System;
using CargoWise.Integration;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Res = Enterprise.ZArchitecture.Core.SourceGenerated.Res;

namespace Enterprise.ZArchitecture.Environment
{
	public class CodeDescriptionPairListRegistryDataType : RegistryDataType<ReadOnlyCodeDescriptionPairList>
	{
		public CodeDescriptionPairListRegistryDataType(int codeMaxLength)
			: this(codeMaxLength, true)
		{
		}

		public CodeDescriptionPairListRegistryDataType(int codeMaxLength, bool isEmptyListAllowed)
			: base(RegistryDataTypes.Codes.Binary, new ReadOnlyCodeDescriptionPairList())
		{
			CodeMaxLength = codeMaxLength;
			AllowDuplicateCodes = true;
			AllowEmptyCodes = true;
			AllowEmptyDescriptions = true;
			AllowDuplicateDescriptions = true;
			IsEmptyListAllowed = isEmptyListAllowed;
		}

		public bool AllowDuplicateCodes { get; set; }
		public bool AllowDuplicateDescriptions { get; set; }
		public bool AllowEmptyCodes { get; set; }
		public bool AllowEmptyDescriptions { get; set; }
		public int CodeMaxLength { get; set; }
		public bool IsEmptyListAllowed { get; set; }
		public bool KeepDefaultValues { get; set; }

		protected override bool ValuesAreEqualCore(ReadOnlyCodeDescriptionPairList a, ReadOnlyCodeDescriptionPairList b)
		{
			return Equals(a, b);
		}

		protected override bool HasDefaultEditorInfoCore => false;

		protected override ReadOnlyCodeDescriptionPairList DeserialiseCore(byte[] value)
		{
			return new ReadOnlyCodeDescriptionPairList(value);
		}

		protected override byte[] SerialiseCore(ReadOnlyCodeDescriptionPairList value)
		{
			return value.ToXMLByteArray();
		}

		protected override void ValidateCore(IRegistryItem registryItem, ReadOnlyCodeDescriptionPairList proposedValue, Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			if (proposedValue != null)
			{
				if (KeepDefaultValues)
				{
					var defaultValues = (ReadOnlyCodeDescriptionPairList)registryItem.DefaultValue;
					foreach (ICodeDescription pair in defaultValues)
					{
						if (!proposedValue.ContainsCode(pair.Code))
						{
							var error = Res.GetString("813390F8-597C-4EF2-808F-302CA2EC1898", "The default code '{0}' has been removed from this list. Default values on this list cannot be removed.", pair.Code);
							throw new RegistryValidationException(error);
						}
					}
				}

				if (proposedValue.Count > 0)
				{
					if (!AllowDuplicateCodes)
					{
						for (var i = 0; i < proposedValue.Count; i++)
						{
							for (var j = 0; j < proposedValue.Count; j++)
							{
								if ((i != j) && string.Equals(proposedValue[i].Code, proposedValue[j].Code, StringComparison.OrdinalIgnoreCase))
								{
									throw new RegistryValidationException(Res.GetString("93cc4908-f6de-44aa-a6e9-29afb774d3f8", "The code '{0}' has been duplicated. Please enter a unique code.", proposedValue[i].Code));
								}
							}
						}
					}

					if (!AllowDuplicateDescriptions)
					{
						for (var i = 0; i < proposedValue.Count; i++)
						{
							for (var j = 0; j < proposedValue.Count; j++)
							{
								if ((i != j) && string.Equals(proposedValue[i].Description, proposedValue[j].Description, StringComparison.OrdinalIgnoreCase))
								{
									throw new RegistryValidationException(Res.GetString("8DBABDD3-9731-40A0-9147-94785599C5DE", "The description '{0}' has been duplicated. Please enter a unique Description.", proposedValue[i].Description));
								}
							}
						}
					}

					foreach (ICodeDescription pair in proposedValue)
					{
						if (pair.Code.Length > CodeMaxLength)
						{
							throw new RegistryValidationException(Res.GetString("093f8420-842e-483d-aa5e-abdb24041c4b", "Record with code '{0}' is incorrect. Code cannot be greater than {1} characters long.", pair.Code, CodeMaxLength.ToString()));
						}

						if (!AllowEmptyCodes && string.IsNullOrEmpty(pair.Code))
						{
							throw new RegistryValidationException(Res.GetString("de11a983-f793-415f-a3bf-8c092fd0ac24", "You cannot enter an item with no Code."));
						}

						if (!AllowEmptyDescriptions && string.IsNullOrEmpty(pair.Description))
						{
							throw new RegistryValidationException(Res.GetString("9be92c27-4fb9-4c07-9ec2-4a847ad9f7cd", "You cannot enter an item with no Description."));
						}
					}
				}
				else if (!IsEmptyListAllowed)
				{
					throw new RegistryValidationException(Res.GetString("e340c37e-c03a-45d8-8b04-b4fe3a7f28d4", "Please enter at least one record for this list."));
				}
			}
		}

		protected override ReadOnlyCodeDescriptionPairList CloneValue(ReadOnlyCodeDescriptionPairList value)
		{
			return (ReadOnlyCodeDescriptionPairList)value.Clone();
		}
	}
}
