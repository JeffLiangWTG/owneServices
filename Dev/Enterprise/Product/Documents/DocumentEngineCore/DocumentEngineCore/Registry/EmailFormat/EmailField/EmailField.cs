using System.Collections;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.DocumentEngineCore.Registry
{
	[XmlSerializerAssembly("Enterprise.DocumentEngineCore.XmlSerializers")]
	public class EmailField : RegistryBusinessObjectTemplate
	{
		#region Schema

		public static class Schema
		{
			public const string Code = "Code";
			public const string Index = "Index";
		}

		#endregion

		public EmailField()
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		public EmailField(ZString index, ZString code)
		{
			this.Index = index;
			this.Code = code;
		}

		#region Validation Overrides

		protected override void RunPreSaveValidationCore()
		{
			ValidateCode();
			ValidateIndex();
			base.RunPreSaveValidationCore();
		}

		public void ValidateCode()
		{
			CodeInfo.ClearAllNotifications();
			if (ParentCollection != null && ParentCollection.GetEmailFieldCountWithCode(Code) != 1)
			{
				CodeInfo.AddError(Res.GetString("149c57a3-78d9-4921-aa2d-0d9cb84ff5a6", "Each field can only appear once in this list. Please select one that does not already exist."));
			}
			else if (Code.Trim().IsEmpty || !EmailFieldsPairList.ContainsCode(code))
			{
				CodeInfo.AddError(Res.GetString("1ac10b47-cfe6-48c6-ba44-f04324c91262", "Please select one of the predefined fields."));
			}
		}

		public void ValidateIndex()
		{
			IndexInfo.ClearAllNotifications();
			if (Index.Trim().IsEmpty)
			{
				IndexInfo.AddError(Res.GetString("54bde4f0-d0a7-4a09-ac1a-a2a727c1e1e9", "Please enter a numeric value."));
			}
			else if (!ZInt.CanParse(Index))
			{
				IndexInfo.AddError(Res.GetString("14ebd274-ad07-4772-b04d-4b691ef5a2cd", "Index must be a numeric value."));
			}
			else if (ParentCollection != null && ParentCollection.GetEmailFieldCountWithIndex(Index) != 1)
			{
				IndexInfo.AddError(Res.GetString("8b090ff2-6e55-4cb4-86f7-57b216bb0b78", "This index has already been used. Please enter one that does not already exist."));
			}
		}

		#endregion

		#region Bound Properties

		#region Code
		[MaxLength(30)]
		public ZString Code
		{
			get { return code; }
			set
			{
				CheckMaximumLength(CodeInfo, value);
				SetNonPersistentPropertyValue(CodeInfo, ref code, value);
				SetIndex();
				if (!IsValidationSuspended)
				{
					ValidateCode();
				}
				CodeInfo.RefreshBinding();
				IndexInfo.RefreshBinding();
			}
		}
		public ZPropertyInfo CodeInfo
		{
			get { return GetZPropertyInfo(Schema.Code); }
		}
		ZString code;
		#endregion

		#region Index
		[MaxLength(3)]
		public ZString Index
		{
			get
			{
				return index;
			}
			set
			{
				CheckMaximumLength(IndexInfo, value);
				index = value;
				if (!IsValidationSuspended)
				{
					ValidateIndex();
				}
				IndexInfo.RefreshBinding();
			}
		}
		public ZPropertyInfo IndexInfo
		{
			get
			{
				return GetZPropertyInfo(Schema.Index);
			}
		}
		ZString index;
		#endregion

		#region LookUps

		public virtual CodeDescriptionPairList EmailFieldsPairList
		{
			get
			{
				return new CodeDescriptionPairList();
			}
		}

		#endregion

		[List("EmailFieldsPairList")]
		public ZString Description
		{
			get => EmailFieldsPairList.GetDescriptionFromCode(Code);
			set => Code = EmailFieldsPairList.GetCodeFromDescription(value);
		}

		public ZPropertyInfo DescriptionInfo => GetWrappedZPropertyInfo(nameof(Description), x => CodeInfo);

		#endregion

		protected void SetIndex()
		{
			if (ParentCollection != null && index.IsEmpty)
			{
				ParentCollection.Sort(new EmailFieldComparer());
				if (ParentCollection.Count == 1)
				{
					index = "1";
				}
				else
				{
					index = "" + (ZInt.Parse(ParentCollection[ParentCollection.Count - 1].Index) + 1);
				}
				ParentCollection.Sort(new EmailFieldComparer());
				IndexInfo.RefreshBinding();
			}
		}

		#region class ComparerForTest

		class EmailFieldComparer : IComparer
		{
			#region IComparer Members

			public int Compare(object x, object y)
			{
				return Compare(x as EmailField, y as EmailField);
			}

			int Compare(EmailField x, EmailField y)
			{
				return string.Compare(x.Index, y.Index);
			}

			#endregion
		}

		#endregion

		protected virtual EmailFieldCollection ParentCollection
		{
			get
			{
				return (EmailFieldCollection)GetParentCollection(this, typeof(EmailFieldCollection));
			}
		}

		internal EmailFieldCollection ParentCollectionInternal => ParentCollection;

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new EmailField();
		}

		#region Write/Read XML

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.Index, Index);
			writer.WriteElementString(Schema.Code, Code);
		}
		protected override void ReadElements(XmlReaderWrapper reader)
		{
			Index = reader.ReadElementString(Schema.Index);
			Code = reader.ReadElementString(Schema.Code);
		}

		#endregion
	}
}
