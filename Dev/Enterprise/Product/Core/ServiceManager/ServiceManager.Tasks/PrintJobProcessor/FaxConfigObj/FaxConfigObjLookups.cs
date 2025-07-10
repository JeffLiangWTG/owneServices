using System.Collections.Generic;
using System.ComponentModel;

using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ServiceManager.Tasks.PrintJobProcessor
{
	public class FaxConfigObjLookups : ZLookups
	{
		public FaxConfigObjLookups(FaxConfigObj parent)
			: base(parent) { }

		public RefCountryCollection Countries
		{
			get
			{
				if (_countries == null)
				{
					_countries = new RefCountryCollection(Factory);
					_countries.ApplySort(RefCountrySchema.RN_Desc.Name, ListSortDirection.Ascending);
				}

				return _countries;
			}
		}

		RefCountryCollection _countries;

		public CodeDescriptionPairList InternationalCallPrefixes
		{
			get
			{
				if (Parent.CountryPhoneInformation != null)
				{
					CodeDescriptionPairList result;
					if (!_internationalCallPrefixes.TryGetValue(Parent.CountryPhoneInformation.CountryIsoCode, out result))
					{
						result = new CodeDescriptionPairList();
						foreach (string prefix in Parent.CountryPhoneInformation.InternationalPrefix.Split(PhoneNumberFormatAndValidation.PrefixSeparator))
						{
							result.AddPair(prefix);
						}
					}

					return result;
				}
				else
				{
					return new CodeDescriptionPairList();
				}
			}
		}

		readonly Dictionary<string, CodeDescriptionPairList> _internationalCallPrefixes = new Dictionary<string, CodeDescriptionPairList>();

		#region Implementation

		protected new FaxConfigObj Parent
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (FaxConfigObj)base.Parent; }
		}

		#endregion
	}
}
