using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class RequireReasonForCLRItem : RegistryBusinessObjectTemplate
	{
		public static class Schema
		{
			public const string Code = "Code";
			public const string Title = "Title";
			public const string ClearingReason = "ClearingReason";
			public const string IsMandatory = "IsMandatory";
		}

		#region Bound Properties

		ZString code;
		[MaxLength(3)]
		public ZString Code
		{
			get => code;
			set
			{
				SetNonPersistentPropertyValue(CodeInfo, ref code, value.Trim());

				if (!IsValidationSuspended)
				{
					ValidateCode();
				}
			}
		}

		public ZPropertyInfo CodeInfo => GetZPropertyInfo(Schema.Code);

		public void ValidateCode()
		{
			CodeInfo.ClearAllNotifications();

			if (Code.IsEmpty)
			{
				CodeInfo.AddError(Res.GetString("36250BEC-8BC2-4628-9EDA-5BEBBFFE2115", "Code should not be empty."));
			}
			else if (Code.Length != 3)
			{
				CodeInfo.AddError(Res.GetString("E3377E7D-9736-40A3-A792-1CECC0D7BDBC", "Code Length should be 3."));
			}
			else if (!Code.ToString().All(char.IsUpper))
			{
				CodeInfo.AddError(Res.GetString("56FA187D-948B-451F-898B-2BF738BF6602", "Code needs to be all capital letters."));
			}
			else if (Code == Constants.RequireReasonForCLRRegistryConstants.Code.Other)
			{
				CodeInfo.AddError(Res.GetString("6f7d68e9-f4f0-4dce-a0f1-ac88c10b9909", "'{0}' is the default Code for the use of inputting a reason not available in the registry.", Code));
			}
			else
			{
				var duplicateCodes = ParentCollections.FirstOrDefault()?.Cast<RequireReasonForCLRItem>().GroupBy(x => x.Code).Where(x => x.Count() > 1).Select(x => x.Key).Where(x => x == Code);

				if (duplicateCodes != null && duplicateCodes.Any())
				{
					CodeInfo.AddError(Res.GetString("29B74040-7068-4935-AB9C-C1A441A9967B", "There are duplicate Codes: {0}.", Code));
				}
			}
		}

		ZString title;
		[MaxLength(50)]
		public ZString Title
		{
			get => title;
			set
			{
				SetNonPersistentPropertyValue(TitleInfo, ref title, value.Trim());

				if (!IsValidationSuspended)
				{
					ValidateTitle();
				}
			}
		}

		public ZPropertyInfo TitleInfo => GetZPropertyInfo(Schema.Title);

		public void ValidateTitle()
		{
			TitleInfo.ClearAllNotifications();

			if (Title.IsEmpty)
			{
				TitleInfo.AddError(Res.GetString("4FF92BB0-C84F-4637-B79F-F80E31F93260", "Title should not be empty."));
			}
		}

		ZString clearingReason;
		[MaxLength(100)]
		public ZString ClearingReason
		{
			get => clearingReason;
			set
			{
				SetNonPersistentPropertyValue(ClearingReasonInfo, ref clearingReason, value.Trim());

				if (!string.IsNullOrEmpty(value))
				{
					IsMandatory = false;
				}

				if (!IsValidationSuspended)
				{
					ClearingReasonInfo.ClearAllNotifications();
					ValidateClearingReason(clearingReason, ClearingReasonInfo);
				}
			}
		}

		public ZPropertyInfo ClearingReasonInfo => GetZPropertyInfo(Schema.ClearingReason);

		public void ValidateClearingReason(ZString reason, ZPropertyInfo reasonInfo)
		{
			var errorMessage = GetErrorMessage(reason);
			if (!string.IsNullOrEmpty(errorMessage))
			{
				reasonInfo.AddError(errorMessage);
			}
		}

		public static string GetErrorMessage(ZString reason)
		{
			string result = null;
			if (!string.IsNullOrEmpty(reason))
			{
				var clearedReasonSplitByWhiteSpace = reason.Trim().Split(System.Array.Empty<char>());
				var amountOfCharacters = ZString.Join(string.Empty, clearedReasonSplitByWhiteSpace).Length;
				var amountOfWords = clearedReasonSplitByWhiteSpace.Length;

				if (amountOfCharacters >= 6 && amountOfWords < 2)
				{
					result = ClearingReasonWordLengthValidationText;
				}
				else if (amountOfCharacters < 6 && amountOfWords >= 2)
				{
					result = ClearingReasonCharacterLengthValidationText;
				}
				else if (amountOfCharacters < 6 && amountOfWords < 2)
				{
					result = ClearingReasonWordAndCharacterLengthValidationText;
				}
			}

			return result;
		}

		ZBool isMandatory;
		[ReadOnlyMember(nameof(IsMandatory_ReadOnly))]
		public ZBool IsMandatory
		{
			get => isMandatory;
			set
			{
				SetNonPersistentPropertyValue(IsMandatoryInfo, ref isMandatory, value);

				if (!IsValidationSuspended)
				{
					ValidateMandatory();
				}
			}
		}

		public ZPropertyInfo IsMandatoryInfo => GetZPropertyInfo(Schema.IsMandatory);

		public void ValidateMandatory()
		{
			IsMandatoryInfo.ClearAllNotifications();

			if (!ClearingReason.IsEmpty && IsMandatory)
			{
				IsMandatoryInfo.AddError(Res.GetString("F888D880-FEA0-4DBC-9FA9-6F00DCE2E51C", "When Description is filled in, Mandatory should be unchecked."));
			}
		}

		bool IsMandatory_ReadOnly => !string.IsNullOrWhiteSpace(ClearingReason);

		#endregion

		protected override void RunPreSaveValidationCore()
		{
			ValidateCode();
			ValidateTitle();
			ValidateMandatory();
			base.RunPreSaveValidationCore();
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new RequireReasonForCLRItem();
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			Code = new ZString(reader.ReadElementString(Schema.Code));
			Title = new ZString(reader.ReadElementString(Schema.Title));
			ClearingReason = new ZString(reader.ReadElementString(Schema.ClearingReason));
			IsMandatory = new ZBool(reader.ReadElementString(Schema.IsMandatory));
		}

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(Schema.Code, Code);
			writer.WriteElementString(Schema.Title, Title);
			writer.WriteElementString(Schema.ClearingReason, ClearingReason);
			writer.WriteElementString(Schema.IsMandatory, IsMandatory.ToString());
		}

		public static ZString ClearingReasonValidationText => Res.GetString("514490FA-C477-4ADC-A25D-047A5F058E6F", "Your organization requires you to enter a reason for clearing this record as it had potential denied party matches. Your reason will be recorded for audit purposes.");

		public static ZString ClearingReasonWordLengthValidationText => Res.GetString("1c25b6bd-a534-47fe-a292-246cea5d5995", "Clearing reason must be minimum two words long.");

		public static ZString ClearingReasonCharacterLengthValidationText => Res.GetString("48b0a6e0-76b4-404a-a863-88ad593e7f97", "Clearing reason must be minimum six characters long.");

		public static ZString ClearingReasonWordAndCharacterLengthValidationText => Res.GetString("f8ce2ed7-0082-4eee-95f2-f43bf6c7ed1c", "Clearing reason must be minimum two words and six characters long.");
	}
}
