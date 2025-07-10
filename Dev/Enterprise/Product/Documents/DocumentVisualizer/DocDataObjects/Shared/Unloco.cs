using System;
using System.Globalization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentVisualizer.DocDataObjects
{
	public sealed class Unloco : DocDataObject, IUnloco
	{
		#region Ctor

		public Unloco(BusinessObjectFactory factory, IRefUNLOCOCollection unlocos, IRefCountryCollection countries)
			: base(factory)
		{
			Unlocos = unlocos ?? throw new ArgumentNullException(nameof(unlocos));
			this.countries = countries ?? throw new ArgumentNullException(nameof(countries));
			this.codeMapper = unlocos as ICodeMapper;
			this.CodeInfo.ValueChanged += OnCodeChanged;
		}

		readonly IRefCountryCollection countries;
		readonly ICodeMapper codeMapper;

		internal void SetNameProvider(Func<IRefUNLOCO, string> nameProvider)
		{
			this.nameProvider = nameProvider;
			var unloco = Factory.LoadFromNaturalKey<IRefUNLOCO>(RefUNLOCOSchema.RL_Code, Code);
			Name = nameProvider(unloco);
		}

		Func<IRefUNLOCO, string> nameProvider;

		#endregion

		#region Create

		public static Unloco Create(IContext context, IRefUNLOCO refUnloco)
		{
			var unloco = new Unloco(context.Factory, context.Unlocos, context.Countries);
			unloco.Name = refUnloco?.RL_PortName ?? ZString.Empty;
			unloco.Country.Code = refUnloco?.RL_RN_NKCountryCode ?? ZString.Empty;
			unloco.IATACode = refUnloco?.RL_IATA ?? ZString.Empty;
			unloco.Code = refUnloco?.RL_Code ?? ZString.Empty;

			return unloco;
		}

		#endregion

		public static Unloco Create(IContext context, IUnloco unloco)
		{
			var result = new Unloco(context.Factory, context.Unlocos, context.Countries);
			result.Name = unloco?.Name ?? ZString.Empty;
			result.Country.Code = unloco?.Country?.Code ?? ZString.Empty;
			result.IATACode = unloco?.IATACode ?? ZString.Empty;
			result.Code = unloco?.Code ?? ZString.Empty;

			return result;
		}

		public static Unloco Create(IContext context, IRefUNLOCO refUnloco, bool codeDisableModifiable)
		{
			var unloco = Create(context, refUnloco);
			unloco.Code_DisableModifiable = codeDisableModifiable;
			return unloco;
		}

		#region Code

		[List(nameof(Unlocos)), MaxLength(NotificationTypes.MessageError, 5), DisableModifiableMember(nameof(Code_DisableModifiable))]
		public ZString Code
		{
			get => code;
			set
			{
				var mappingCode = value;
				if (codeMapper != null)
				{
					mappingCode = codeMapper.ShowForeignCode ? codeMapper.GetForeignCode(value) : codeMapper.GetLocalCode(value);
				}

				if (SetNonPersistentPropertyValue(CodeInfo, ref code, mappingCode.ToUpperInvariant()))
				{
					Validate(CodeInfo);
				}
			}
		}

		ZString code;

		public ZPropertyInfo CodeInfo => GetZPropertyInfo(nameof(Code));

		void OnCodeChanged(object sender, EventArgs args)
		{
			var unloco = Factory.LoadFromNaturalKey<IRefUNLOCO>(RefUNLOCOSchema.RL_Code, code);
			if (unloco == null && codeMapper != null && codeMapper.ShowForeignCode)
			{
				var localCode = codeMapper.GetLocalCode(code);
				if (!string.IsNullOrEmpty(localCode))
				{
					unloco = Factory.LoadFromNaturalKey<IRefUNLOCO>(RefUNLOCOSchema.RL_Code, localCode);
				}
			}

			Name = nameProvider?.Invoke(unloco) ?? unloco?.RL_PortName;
			Country.Code = unloco?.RL_RN_NKCountryCode ?? ZString.Empty;
			IATACode = unloco?.RL_IATA ?? ZString.Empty;
		}

		#endregion

		public IRefUNLOCOCollection Unlocos { get; }

		public bool Code_DisableModifiable { get; set; }

		#region Name

		[MaxLength(NotificationTypes.MessageError, 35)]
		public ZString Name
		{
			get => name;
			set
			{
				if (SetNonPersistentPropertyValue(NameInfo, ref name, value))
				{
					Validate(NameInfo);
				}
			}
		}

		ZString name;

		public ZPropertyInfo NameInfo => GetZPropertyInfo(nameof(Name));

		#endregion

		#region Country

		ICountry IUnloco.Country => Country;

		public Country Country
		{
			get
			{
				if (country == null)
				{
					country = SetChild(null, new Country(Factory, countries));
				}

				return country;
			}
		}
		Country country;

		#endregion

		#region IATACode

		[MaxLength(NotificationTypes.MessageError, 3)]
		public ZString IATACode
		{
			get => iataCode;
			set
			{
				if (SetNonPersistentPropertyValue(IATACodeInfo, ref iataCode, value.ToUpperInvariant()))
				{
					Validate(IATACodeInfo);
				}
			}
		}

		ZString iataCode;

		public ZPropertyInfo IATACodeInfo => GetZPropertyInfo(nameof(IATACode));

		#endregion

		#region Implementation

		public override string ToString() => string.Format(CultureInfo.InvariantCulture, "{0} - {1}", Code, Name);

		#endregion
	}
}
