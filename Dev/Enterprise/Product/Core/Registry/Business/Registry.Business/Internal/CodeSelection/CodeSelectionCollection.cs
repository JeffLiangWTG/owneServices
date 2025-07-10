using System.Linq;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class CodeSelectionCollection : RegistryBusinessObjectCollectionTemplate, ICodeDescriptionPairList
	{
		CodeDescriptionPairListProvider codesProvider;

		public CodeSelectionCollection()
		{
		}

		public CodeSelectionCollection(CodeDescriptionPairListProvider codesProvider)
		{
			SetCodesProvider(codesProvider);
		}

		public new CodeSelection this[int index]
		{
			get { return (CodeSelection)base[index]; }
		}

		public ICodeDescriptionPairList Codes
		{
			get { return codesProvider.CodeDescriptionPairList; }
		}

		public new CodeSelection AddNew()
		{
			return (CodeSelection)base.AddNew();
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new CodeSelectionCollection(codesProvider);
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new CodeSelection();
		}

		public void SetCodesProvider(CodeDescriptionPairListProvider value)
		{
			codesProvider = value;
		}

		#region ICodeDescriptionPairList Members

		bool ICodeDescriptionPairList.ContainsCode(object code)
		{
			if (code != null)
			{
				foreach (CodeSelection selection in this)
				{
					if (selection.Code == code.ToString())
					{
						return true;
					}
				}
			}

			return false;
		}

		string ICodeDescriptionPairList.GetDescriptionFromCode(string code)
		{
			foreach (CodeSelection selection in this)
			{
				if (selection.Code == code)
				{
					return selection.Description;
				}
			}

			return "";
		}

		#endregion

		public override bool Equals(object obj)
		{
			if (base.Equals(obj))
			{
				return true;
			}

			ICodeDescriptionPairList other = obj as ICodeDescriptionPairList;

			if (other == null)
			{
				return false;
			}

			if (other.Count == 0 && this.Count == 0)
			{
				return true;
			}

			if (other.Count != this.Count)
			{
				return false;
			}

			foreach (CodeSelection selection in this)
			{
				if (!other.ContainsCode(selection.Code))
				{
					return false;
				}
			}

			return true;
		}

		public static bool operator ==(CodeSelectionCollection lhs, object rhs)
		{
			if (((object)lhs) == null)
			{
				return rhs == null;
			}

			return lhs.Equals(rhs);
		}

		public static bool operator !=(CodeSelectionCollection lhs, object rhs)
		{
			if (((object)lhs) == null)
			{
				return rhs != null;
			}

			return !lhs.Equals(rhs);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int hash = 0;
				foreach (CodeSelection selection in this.AsEnumerable().Cast<CodeSelection>().OrderBy(x => x.Code))
				{
					hash = hash * 139 + selection.Code.GetHashCode();
				}
				return hash;
			}
		}
	}
}
