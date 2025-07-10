using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.ZArchitecture.Business
{
	public class ConversionFactorViewModel : NonPersistentBusinessObject
	{
		public static readonly int NumberOfDecimals = 2;

		public static class Schema
		{
			public static readonly string ConversionFactorString = "ConversionFactorString";
		}

		public ConversionFactorViewModel(Func<ConversionFactorViewModel, IConversionFactorLookups> lookupsProvider, Func<ConversionFactorViewModel, IConversionFactorValidation> validationProvider)
		{
			this.lookupsProvider = Argument.NotNull(lookupsProvider, "lookupsProvider");
			this.validationProvider = Argument.NotNull(validationProvider, "validationProvider");
		}

		#region Properties

		#region ConversionFactorString

		[List("Lookups.ConversionFactors")]
		[ResourceStringData("ConversionFactorViewModel|ConversionFactorString", Caption = "Conversion Factor", ShortCaption = "Factor", FullDescription = "The default Conversion Factor to be used when converting between weight/volume units for this Trade Lane.")]
		[SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults")]
		public ZString ConversionFactorString
		{
			get
			{
				return conversionFactorString;
			}
			set
			{
				if (ConversionFactor.TryParse(value, out ConversionFactor factor))
				{
					conversionFactorString = factor.ToShortString();
				}
				else
				{
					conversionFactorString = value;
				}

				conversionFactor = factor;

				if (!IsValidationSuspended)
				{
					Validation.ValidateConversionFactorString();
				}

				ConversionFactorStringInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo ConversionFactorStringInfo
		{
			[DebuggerStepThrough]
			get { return GetZPropertyInfo(Schema.ConversionFactorString); }
		}

		ZString conversionFactorString;

		#endregion

		#region Conversion Factor

		public ConversionFactor ConversionFactor
		{
			get
			{
				return conversionFactor;
			}
			set
			{
				conversionFactor = value;
				conversionFactorString = value.IsEmpty ? string.Empty : value.ToShortString();

				ConversionFactorStringInfo.RefreshBinding();
			}
		}

		ConversionFactor conversionFactor;

		#endregion

		public IConversionFactorLookups Lookups => lookupsProvider(this);
		public IConversionFactorValidation Validation => validationProvider(this);

		readonly Func<ConversionFactorViewModel, IConversionFactorValidation> validationProvider;
		readonly Func<ConversionFactorViewModel, IConversionFactorLookups> lookupsProvider;
		#endregion
	}
}
