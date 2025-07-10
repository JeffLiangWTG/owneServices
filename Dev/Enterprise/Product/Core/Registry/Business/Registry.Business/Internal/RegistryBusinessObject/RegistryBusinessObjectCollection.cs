using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Business.Internal;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public abstract class RegistryBusinessObjectCollection : RegistryBusinessObjectCollectionTemplate
	{
		protected RegistryBusinessObjectCollection()
			: this(null)
		{
		}

		protected RegistryBusinessObjectCollection(BusinessObjectFactory factory)
			: this(null, factory)
		{
		}

		protected RegistryBusinessObjectCollection(ReadOnlyCodeDescriptionPairList list, int codeMaxLength = 0)
			: this(null, null, list, codeMaxLength)
		{
		}

		protected RegistryBusinessObjectCollection(FallbackLevel fallbackLevel, BusinessObjectFactory factory = null, ReadOnlyCodeDescriptionPairList list = null, int codeMaxLength = 0)
			: base(fallbackLevel, factory)
		{
			this.CodeMaxLength = codeMaxLength;
			if (list != null)
			{
				foreach (ICodeDescription pair in list)
				{
					RegistryBusinessObject element = AddNew();
					FillNewElementFromCodeDescriptionPair(pair, element);
				}
			}
		}

		public new RegistryBusinessObject this[int index] => (RegistryBusinessObject)Elements[index];

		public new RegistryBusinessObject AddNew() => (RegistryBusinessObject)base.AddNew();

		protected override IFindBoxListProvider FindBoxListProvider
		{
			get
			{
				return new RegistryBusinessObjectCollectionFindBoxListProvider(this);
			}
		}

		protected internal int CodeMaxLength;

		void FillNewElementFromCodeDescriptionPair(ICodeDescription pair, RegistryBusinessObject child)
		{
			FillNewElementFromCodeDescriptionPairCore(pair, child);

			child.CodeMaxLength = CodeMaxLength;
			child.Code = pair.Code;
			child.Description = ((IMultilingualDescription)pair).MultilingualDescription;
		}

		protected virtual void FillNewElementFromCodeDescriptionPairCore(ICodeDescription pair, RegistryBusinessObject child)
		{
		}

		public string GetCodeFromDescription(string description)
		{
			return GetCodeFromDescriptionCore(description);
		}

		protected virtual string GetCodeFromDescriptionCore(string description)
		{
			foreach (RegistryBusinessObject element in this)
			{
				if (element.Description.ToString(Env.CurrentUser.Language) == description)
				{
					return element.Code;
				}
			}
			return string.Empty;
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			RegistryBusinessObject newChild = child as RegistryBusinessObject;
			newChild.CodeMaxLength = CodeMaxLength;
		}

		public bool ContainsCode(string code)
		{
			return FindByCode(code) != null;
		}

		public RegistryBusinessObject FindByCode(string code)
		{
			return this.Cast<RegistryBusinessObject>().FirstOrDefault(element => string.Compare(element.Code, code, IgnoreCaseInCodes) == 0);
		}

		protected virtual bool IgnoreCaseInCodes { get { return false; } }

		public CodeDescriptionPairList GetCodeDescriptionPairList()
		{
			return GetCodeDescriptionPairListCore();
		}

		protected virtual CodeDescriptionPairList GetCodeDescriptionPairListCore()
		{
			CodeDescriptionPairList result = new CodeDescriptionPairList();

			foreach (RegistryBusinessObject element in this)
			{
				result.AddPair(element.Code, element.Description);
			}

			return result;
		}
	}

	public class RegistryBusinessObjectCollectionFindBoxListProvider : NonPersistentBusinessObjectFindBoxListProvider
	{
		public RegistryBusinessObjectCollectionFindBoxListProvider(RegistryBusinessObjectCollection collection)
			: base(collection)
		{
		}

		public override string CodeFromDescription(string description)
		{
			return ((RegistryBusinessObjectCollection)List).GetCodeFromDescription(description);
		}

		public override string CodeFromPrimaryKey(ZGuid pK) => string.Empty;

		public override string DescriptionFromPrimaryKey(ZGuid pK) => string.Empty;
	}
}
