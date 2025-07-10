using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using CargoWise.Windows.UI.Testing;

namespace Enterprise.ZArchitecture.GUI.Internal
{
	[SuppressFormDesignerAnalysis]
	[CompositeFieldControl]
	public abstract class ZListProviderFindBox : ZAutoCompleteFindBox, IFindBoxListProvider
	{
		protected override IFindBoxListProvider ListProvider
		{
			get { return this; }
		}

		#region IFindBoxListProvider Members

		(string, bool) IFindBoxListProvider.NearestMatch(string code, bool explicitAutoComplete, int cursor)
		{
			if (IFindBox.ListProvider.List is IFindBoxListProvider listProvider)
			{
				return listProvider.NearestMatch(code, explicitAutoComplete, cursor);
			}
			return (code, false);
		}

		(string, bool) IFindBoxListProvider.NearestMatchCore(string code, bool explicitAutoComplete)
		{
			if (IFindBox.ListProvider.List is IFindBoxListProvider listProvider)
			{
				return listProvider.NearestMatchCore(code, explicitAutoComplete);
			}
			return (code, false);
		}

		ZGuid IFindBoxListProvider.PrimaryKeyFromCode(string code)
		{
			if (IFindBox.ListProvider.List is IFindBoxListProvider listProvider)
			{
				return listProvider.PrimaryKeyFromCode(code);
			}
			return ZGuid.Empty;
		}

		string IFindBoxListProvider.CodeFromPrimaryKey(ZGuid pK)
		{
			if (IFindBox.ListProvider.List is IFindBoxListProvider listProvider)
			{
				return listProvider.CodeFromPrimaryKey(pK);
			}
			return string.Empty;
		}

		string IFindBoxListProvider.DescriptionFromCode(string code)
		{
			if (IFindBox.ListProvider.List is IFindBoxListProvider listProvider)
			{
				return listProvider.DescriptionFromCode(code);
			}
			return null;
		}

		string IFindBoxListProvider.DescriptionFromPrimaryKey(ZGuid pK)
		{
			if (IFindBox.ListProvider.List is IFindBoxListProvider listProvider)
			{
				return listProvider.DescriptionFromPrimaryKey(pK);
			}
			return null;
		}

		IEnumerable<BusinessObject> IFindBoxListProvider.GetBusinessObjectsFromCode(string code)
		{
			if (IFindBox.ListProvider.List is IFindBoxListProvider listProvider)
			{
				return listProvider.GetBusinessObjectsFromCode(code);
			}
			return Enumerable.Empty<BusinessObject>();
		}

		BusinessObject IFindBoxListProvider.GetBusinessObjectFromCode(string code)
		{
			if (IFindBox.ListProvider.List is IFindBoxListProvider listProvider)
			{
				return listProvider.GetBusinessObjectFromCode(code);
			}
			return null;
		}

		IEnumerable<BusinessObject> IFindBoxListProvider.GetBusinessObjectsFromCodeWithoutFilter(string code)
		{
			if (IFindBox.ListProvider.List is IFindBoxListProvider listProvider)
			{
				return listProvider.GetBusinessObjectsFromCodeWithoutFilter(code);
			}
			return Enumerable.Empty<BusinessObject>();
		}

		BusinessObject IFindBoxListProvider.GetBusinessObjectFromCodeWithoutFilter(string code)
		{
			if (IFindBox.ListProvider.List is IFindBoxListProvider listProvider)
			{
				return listProvider.GetBusinessObjectFromCodeWithoutFilter(code);
			}
			return null;
		}

		ICodeDescription IFindBoxListProvider.GetCustomCodeDescription(BusinessObject bizo)
		{
			if (IFindBox.ListProvider.List is IFindBoxListProvider listProvider)
			{
				return listProvider.GetCustomCodeDescription(bizo);
			}
			return bizo;
		}

		bool IFindBoxListProvider.AutoCompleteOnCommit
		{
			get
			{
				if (IFindBox.ListProvider.List is IFindBoxListProvider listProvider)
				{
					return listProvider.AutoCompleteOnCommit;
				}
				return false;
			}
		}

		IBusinessObjectCollection IFindBoxListProvider.List
		{
			get
			{
				var list = List as IBusinessObjectCollection;
				if (list != null)
				{
					list.ListPropertyDescriptor = ListPropertyDescriptor;
					list.Parent = DataSource;
				}
				return list;
			}
		}
	}

	#endregion
}
