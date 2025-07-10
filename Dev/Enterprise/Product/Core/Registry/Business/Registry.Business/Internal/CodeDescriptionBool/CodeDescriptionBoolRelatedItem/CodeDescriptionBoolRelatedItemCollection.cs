using System;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Registry.Business
{
	#region ICodeDescriptionBoolRelatedItemList Interface

	public interface ICodeDescriptionBoolRelatedItemList : ICodeDescriptionBoolList
	{
		new ICodeDescriptionBoolRelatedItem this[int i] { get; }
		string GetRelatedItemFromCode(string code);
	}

	#endregion
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]

	public class CodeDescriptionBoolRelatedItemCollection : CodeDescriptionBoolCollection, ICodeDescriptionBoolRelatedItemList
	{
		public CodeDescriptionBoolRelatedItemCollection()
			: this(0)
		{
		}

		public CodeDescriptionBoolRelatedItemCollection(int codeMaxLength)
			: base(null, true, codeMaxLength)
		{
		}

		public CodeDescriptionBoolRelatedItemCollection(ReadOnlyCodeDescriptionPairList relatedItemLookup)
			: this(relatedItemLookup, 0)
		{
		}

		public CodeDescriptionBoolRelatedItemCollection(ReadOnlyCodeDescriptionPairList relatedItemLookup, int codeMaxLength)
			: this(codeMaxLength)
		{
			this.RelatedItemLookup = relatedItemLookup;
		}

		public new CodeDescriptionBoolRelatedItem Add(ZString code, MultilingualString description)
		{
			return Add(code, description, true, "");
		}

		public new CodeDescriptionBoolRelatedItem Add(ZString code, MultilingualString description, bool booleanValue)
		{
			return Add(code, description, booleanValue, "");
		}

		public CodeDescriptionBoolRelatedItem Add(ZString code, MultilingualString description, bool booleanValue, ZString relatedItemCode)
		{
			CodeDescriptionBoolRelatedItem result = AddNew();
			result.CodeMaxLength = CodeMaxLength;
			result.Code = code;
			result.Description = description;
			result.Bool = booleanValue;
			result.RelatedItemCode = relatedItemCode;
			return result;
		}

		protected override CodeDescriptionBoolCollection GetNewCollection()
		{
			return new CodeDescriptionBoolRelatedItemCollection(RelatedItemLookup);
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			((CodeDescriptionBoolRelatedItem)child).Bool = true;
			((CodeDescriptionBoolRelatedItem)child).RelatedItemCode = "";
		}

		protected override BusinessObject AddNewCore()
		{
			var newItem = (CodeDescriptionBoolRelatedItem)base.AddNewCore();
			newItem.Parent = this;
			return newItem;
		}

		public override void Add(BusinessObject businessObject)
		{
			var item = businessObject as CodeDescriptionBoolRelatedItem;
			if (item != null)
			{
				item.Parent = this;
			}
			base.Add(businessObject);
		}

		public new CodeDescriptionBoolRelatedItem this[int index]
		{
			get { return (CodeDescriptionBoolRelatedItem)Elements[index]; }
		}

		public new CodeDescriptionBoolRelatedItem AddNew()
		{
			return (CodeDescriptionBoolRelatedItem)base.AddNew();
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new CodeDescriptionBoolRelatedItem(this);
		}

		#region Related Item List

		public CodeDescriptionPairList GetRelatedItemList(string relatedItemCode)
		{
			CodeDescriptionPairList result = null;

			foreach (ICodeDescriptionBoolRelatedItem element in this)
			{
				if (string.Compare(element.Code, relatedItemCode, true) == 0)
				{
					result = new OpportunitySourceRelatedItemProvider().GetRelatedItemList(element.RelatedItemCode);
					if (result != null)
					{
						break;
					}
				}
			}

			return result ?? new CodeDescriptionPairList();
		}

		public CodeDescriptionPairList GetActiveRelatedItemList(string relatedItemCode)
		{
			CodeDescriptionPairList result = null;

			foreach (ICodeDescriptionBoolRelatedItem element in this)
			{
				if (string.Compare(element.Code, relatedItemCode, true) == 0)
				{
					result = new OpportunitySourceRelatedItemProvider().GetActiveRelatedItemList(element.RelatedItemCode);
					if (result != null)
					{
						break;
					}
				}
			}
			return result ?? new CodeDescriptionPairList();
		}

		public string GetRelatedItemFromCode(string code)
		{
			string trimmedCode = code.Trim();
			foreach (ICodeDescriptionBoolRelatedItem element in this)
			{
				if (string.Equals(element.Code.Trim(), trimmedCode, StringComparison.OrdinalIgnoreCase))
				{
					return element.RelatedItemCode;
				}
			}
			return null;
		}

		ICodeDescriptionBoolRelatedItem ICodeDescriptionBoolRelatedItemList.this[int i]
		{
			get { return this[i]; }
		}
		#endregion

		#region Related Item Lookup

		public ReadOnlyCodeDescriptionPairList RelatedItemLookup;

		#endregion
	}
}
