using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using UniversalRegistrationNumber = Enterprise.UniversalDataBuss.DataObjects.Universal.RegistrationNumber;

namespace Enterprise.DocumentVisualizer.DocDataObjects
{
	public class RegistrationNumber : DocDataObject, IRegistrationNumber
	{
		#region Create

		public static RegistrationNumber Create(IContext context, UniversalRegistrationNumber universalRegistrationNumber)
		{
			_ = context ?? throw new ArgumentNullException(nameof(context));

			return new RegistrationNumber
			{
				CountryOfIssue = Country.Create(context, universalRegistrationNumber?.CountryOfIssue),
				Value = universalRegistrationNumber?.Value.GetValueOrDefault() ?? ZString.Empty,
				Type = CodeDescription.Create(universalRegistrationNumber?.Type),
			};
		}

		#endregion

		#region Type

		public ICodeDescription Type
		{
			get => type;
			set => type = SetChild(type, value);
		}

		ICodeDescription type;

		#endregion

		#region CountryOfIssue

		public ICountry CountryOfIssue
		{
			get => countryOfIssue;
			set => countryOfIssue = SetChild(countryOfIssue, value);
		}

		ICountry countryOfIssue;

		#endregion

		#region Value

		public ZString Value
		{
			get => _value;
			set
			{
				if (SetNonPersistentPropertyValue(ValueInfo, ref _value, value))
				{
					Validate(ValueInfo);
				}
			}
		}

		ZString _value;

		public ZPropertyInfo ValueInfo => GetZPropertyInfo(nameof(Value));

		#endregion
	}
}
