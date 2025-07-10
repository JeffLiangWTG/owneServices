using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public class SingleLineEntry : AutoSingleLineEntry
	{
		public SingleLineEntry(BusinessObjectFactory factory)
			: base(factory) { }

		public SingleLineEntry(JobDeclaration declaration)
			: this(declaration.Factory)
		{
			this.declaration = declaration;
		}

		public SingleLineEntryLookups Lookups
		{
			get
			{
				if (lookups == null || !IsLookupsCachedInBase)
				{
					lookups = GetNewLookups();
				}

				return lookups;
			}
		}

		[List(nameof(Lookups) + "." + nameof(SingleLineEntryLookups.Currencies))]
		public override ZGuid Currency
		{
			get => base.Currency;
			set => base.Currency = value;
		}

		[BusinessObjectTestExclude]
		[List(nameof(Lookups) + "." + nameof(SingleLineEntryLookups.Tariffs))]
		public override ZString TariffNumber
		{
			get => TariffFormatter.New(declaration?.CountryCode).DisplayFormat(base.TariffNumber);
			set => base.TariffNumber = TariffFormatter.New(declaration?.CountryCode).Format(value);
		}

		[List(nameof(Lookups) + "." + nameof(SingleLineEntryLookups.CPCList))]
		public override ZString CPCCode
		{
			get => base.CPCCode;
			set => base.CPCCode = value;
		}

		[ReadOnlyMember(nameof(CreateLIC99ReadOnly))]
		public override ZBool CreateLIC99
		{
			get => base.CreateLIC99;
			set => base.CreateLIC99 = value;
		}

		public bool CreateLIC99ReadOnly => declaration == null || declaration.IsImport;

		public ZString DataGrouping => declaration?.GetDefaultDataGroupingCode() ?? ZString.Empty;

		public ZString TariffType => TariffFormatter.GetTariffType(declaration?.IsExport ?? false);

		public ZDateTime EffectiveDate => declaration?.DateOfValuation ?? ZDateTime.Today;

		protected readonly JobDeclaration declaration;

		protected virtual SingleLineEntryLookups GetNewLookups() => new SingleLineEntryLookups(this);

		SingleLineEntryLookups lookups;
	}
}
