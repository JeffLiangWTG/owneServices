using System;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	#region ICodeDescriptionBoolRelatedItem Interface

	public interface ICodeDescriptionBoolRelatedItem : ICodeDescriptionBool
	{
		ZString RelatedItemCode { get; }
	}

	#endregion

	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class CodeDescriptionBoolRelatedItem : CodeDescriptionBool, ICodeDescriptionBoolRelatedItem
	{
		#region Schema

		protected new abstract class Schema : RegistryBusinessObject.Schema
		{
			public const string RelatedItemCode = "RelatedItemCode";
		}

		#endregion

		public CodeDescriptionBoolRelatedItem()
		{
		}

		public CodeDescriptionBoolRelatedItem(CodeDescriptionBoolRelatedItemCollection parent)
		{
			this.Parent = parent;
		}

		protected override void SetCustomDefaultValuesCore()
		{
			base.SetCustomDefaultValuesCore();
			RelatedItemCode = string.Empty;
		}

		#region Clone

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new CodeDescriptionBoolRelatedItem(Parent);
		}

		protected override void CopyValuesToClone(RegistryBusinessObjectTemplate clone)
		{
			base.CopyValuesToClone(clone);
			((CodeDescriptionBoolRelatedItem)clone).RelatedItemCode = RelatedItemCode;
		}

		#endregion

		#region Related Item

		[MaxLength(10)]
		public virtual ZString RelatedItemCode
		{
			get { return relatedItemCode; }
			set
			{
				CheckMaximumLength(RelatedItemCodeInfo, value);
				SetNonPersistentPropertyValue(RelatedItemCodeInfo, ref relatedItemCode, value);
			}
		}
		ZString relatedItemCode;

		public ZPropertyInfo RelatedItemCodeInfo
		{
			get { return GetZPropertyInfo(Schema.RelatedItemCode); }
		}

		[List("RelatedItemDescriptionCodeList")]
		[MaxLength(200)]
		public ZString RelatedItemDescription
		{
			get
			{
				ZString result = relatedItemDescription;
				if (result.IsEmpty)
				{
					var list = RelatedItemDescriptionCodeList;
					var description = list.GetCodeFromDescription(RelatedItemCode);
					if (!string.IsNullOrEmpty(description))
					{
						int index = list.IndexOfCode(description);
						if (index > -1)
						{
							CodeDescriptionPair element = list[index] as CodeDescriptionPair;
							result = element.MultilingualCode;
						}
					}
				}
				return result;
			}
			set
			{
				SetNonPersistentPropertyValue(RelatedItemDescriptionInfo, ref relatedItemDescription, value);
				ValidateRelatedItemDescription();

				if (value.IsEmpty || RelatedItemDescriptionInfo.HasErrors())
				{
					RelatedItemCode = ZString.Empty;
				}
				else
				{
					foreach (CodeDescriptionPair element in RelatedItemDescriptionCodeList)
					{
						if (String.Equals(element.MultilingualCode.ToString().Trim(), value.Trim(), StringComparison.OrdinalIgnoreCase))
						{
							RelatedItemCode = element.Description;
						}
					}
				}
			}
		}
		ZString relatedItemDescription;

		public ZPropertyInfo RelatedItemDescriptionInfo
		{
			get { return GetZPropertyInfo(nameof(RelatedItemDescription)); }
		}

		public void ValidateRelatedItemDescription()
		{
			RelatedItemDescriptionInfo.ClearAllNotifications();

			bool found = RelatedItemDescription.Trim().IsEmpty;
			if (!found)
			{
				foreach (CodeDescriptionPair element in RelatedItemDescriptionCodeList)
				{
					if (String.Equals(element.MultilingualCode.ToString().Trim(), RelatedItemDescription.Trim(), StringComparison.OrdinalIgnoreCase))
					{
						found = true;
						break;
					}
				}
			}

			if (!found)
			{
				RelatedItemDescriptionInfo.AddError(ListValidation.GetNotificationMessage(RelatedItemDescriptionInfo).ToString());
			}
		}

		#endregion

		#region Lookups

		public ReadOnlyCodeDescriptionPairList RelatedItemDescriptionCodeList
		{
			get
			{
				return Parent == null ? new CodeDescriptionPairList() :
							 (Parent.RelatedItemLookup ?? new CodeDescriptionPairList());
			}
		}

		#endregion

		[XmlIgnore]
		[BusinessObjectTestExclude]
		public CodeDescriptionBoolRelatedItemCollection Parent
		{
			get;
			set;
		}

		#region Xml Serialisation

		protected override void ReadMoreElements(XmlReader reader)
		{
			base.ReadMoreElements(reader);
			relatedItemCode = reader.ReadElementString(Schema.RelatedItemCode);
		}

		protected override void WriteMoreElements(XmlWriter writer)
		{
			base.WriteMoreElements(writer);
			writer.WriteElementString(Schema.RelatedItemCode, RelatedItemCode);
		}

		#endregion
	}
}
