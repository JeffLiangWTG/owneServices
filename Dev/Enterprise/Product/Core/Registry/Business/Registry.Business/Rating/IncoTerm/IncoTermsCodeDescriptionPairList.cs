using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

using Incoterms = Enterprise.Core.Constants.IncoTerms;

namespace Enterprise.Registry.Business
{
	public class IncoTermsCodeDescriptionPairList : UntranslatableCodeDescriptionPairList
	{
		public IncoTermsCodeDescriptionPairList()
			: this(IncoTermsListType.Default)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Untranslatable reason")]
		public IncoTermsCodeDescriptionPairList(IncoTermsListType type)
			: base("Incoterms should always be displayed in English")
		{
			switch (type)
			{
				case IncoTermsListType.Default:
					AddAllIncoterms();
					break;

				case IncoTermsListType.IncoTerms2000:
					AddAllCodesFromCollection(Incoterms.Incoterms2000);
					break;

				case IncoTermsListType.IncoTerms2010:
					AddAllCodesFromCollection(Incoterms.Incoterms2010);
					break;

				case IncoTermsListType.IncoTerms2020:
					AddAllCodesFromCollection(Incoterms.Incoterms2020);
					break;

				case IncoTermsListType.ActiveIncoTerms:
					AddLatestIncoterms();
					break;

				case IncoTermsListType.ActiveIncludingDomesticTerms:
					AddLatestIncoterms();
					AddRange(new CodeDescriptionPairList(OLookUpEditType.DomesticPaymentTerms));
					break;
			}

			Sort();
		}

		void AddAllIncoterms()
		{
			foreach (var pair in activeIncotermDateCollectionPairs)
			{
				AddAllCodesFromCollection(pair.collection);
			}
		}

		void AddLatestIncoterms()
		{
			foreach (var (date, collection) in activeIncotermDateCollectionPairs)
			{
				if (ZDateTime.UtcNow >= date)
				{
					AddAllCodesFromCollection(collection);
					return;
				}
			}
		}

		void AddAllCodesFromCollection(IEnumerable<string> collection)
		{
			foreach (var code in collection)
			{
				var description = GetRegistryDescriptionFromCode(code);

				AddPairIfNotExist(code, description);
			}
		}

		MultilingualString GetRegistryDescriptionFromCode(string code)
		{
			return IncotermRegistryOverrides
				.FirstOrDefault(c => c.IncoTerm == code)
				?.IncoTermDescriptionMultilingual ?? Incoterms.Descriptions.DefaultCodeDescriptionPairs[code];
		}

		IEnumerable<IncoTermChargeCodes> incotermRegistryOverrides;
		IEnumerable<IncoTermChargeCodes> IncotermRegistryOverrides
			=> incotermRegistryOverrides ?? (incotermRegistryOverrides = RatingDataRegistry.Instance.IncoTermDefinition.Value.Cast<IncoTermChargeCodes>());

		readonly (DateTime date, IEnumerable<string> collection)[] activeIncotermDateCollectionPairs = new (DateTime, IEnumerable<string>)[]
		{
			(Incoterms.Incoterms2020EffectiveDate, Incoterms.Incoterms2020.Union(Incoterms.Incoterms2010)),
			(Incoterms.Incoterms2010EffectiveDate, Incoterms.Incoterms2010),
			(Incoterms.Incoterms2000EffectiveDate, Incoterms.Incoterms2000)
		};
	}

	public class IncoTermsCodeDescriptionPairListProvider : ICodeDescriptionPairListProvider
	{
		public IncoTermsCodeDescriptionPairListProvider(IncoTermsListType type)
		{
			this.type = type;
		}

		public CodeDescriptionPairList CodeDescriptionPairList
		{
			get { return new IncoTermsCodeDescriptionPairList(type); }
		}

		readonly IncoTermsListType type;
	}

	public enum IncoTermsListType
	{
		Default,
		IncoTerms2000,
		IncoTerms2010,
		IncoTerms2020,
		ActiveIncoTerms,
		ActiveIncludingDomesticTerms
	}
}
