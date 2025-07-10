using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public class OIDCConfigRegistryItem : StronglyTypedRegistryItem<OIDCConfig>
	{
		public OIDCConfigRegistryItem(
			string name,
			MultilingualString category,
			MultilingualString caption,
			MultilingualString hint,
			bool isWinzorConfig,
			RegistryStorageFlags storage,
			RegistryOptions options,
			OIDCConfig defaultValue)
		: base(new RegistryItemImpl(name, category, caption, hint, new OIDCConfigRegistryDataType(OIDCConfig.DefaultValue, isWinzorConfig), storage, options, defaultValue))
		{
		}

		public OIDCConfigRegistryItem(
			string name,
			MultilingualString category,
			MultilingualString caption,
			MultilingualString hint,
			RegistryStorageFlags storage,
			RegistryOptions options,
			OIDCConfig defaultValue)
		: base(new RegistryItemImpl(name, category, caption, hint, new OIDCConfigRegistryDataType(OIDCConfig.DefaultValue), storage, options, defaultValue))
		{
		}
	}

	[RegistryEditor("Enterprise.Registry.GUI.OIDCRegistryItemEditor, Enterprise.Registry.GUI")]
	public class OIDCConfigRegistryDataType : NonPersistentBusinessObjectRegistryDataType<OIDCConfig>
	{
		public readonly bool IsWinzorConfig;
		public OIDCConfigRegistryDataType(OIDCConfig defaultValue, bool isWinzorConfig)
			: base(defaultValue)
		{
			this.IsWinzorConfig = isWinzorConfig;
		}

		public OIDCConfigRegistryDataType(OIDCConfig defaultValue)
			: this(defaultValue, false)
		{
		}

		protected override OIDCConfig CloneValue(OIDCConfig value)
		{
			return value.Clone();
		}

		protected override void ValidateCore(
			IRegistryItem registryItem,
			OIDCConfig proposedValue,
			Guid companyPK,
			Guid branchPK,
			Guid departmentPK)
		{
			var notVerifiedMessage = Res.GetString("C657C786-B5EB-45F1-BB26-CCBBD6BF97A2", "Configuration must be verified before changes can be saved");
			proposedValue.RemoveRowError(notVerifiedMessage);

			base.ValidateCore(registryItem, proposedValue, companyPK, branchPK, departmentPK);
	
			if (proposedValue.IsOIDCEnabled && proposedValue.ClaimsMappings.Count == 0)
			{
				throw new RegistryValidationException(Res.GetString("2216781E-D4FE-4BF6-A75D-29F0F6635624", "A Claim Mapping must be specified."));
			}

			var seenClaims = new HashSet<string>();
			var seenIdentifiers = new HashSet<string>();
			var claimNameMustBeUniqueError = Res.GetString("627314C9-23C7-4DBC-BB0A-C75A0B276CE9", "Claim Names must be unique.");
			var identifierMustBeUniqueError = Res.GetString("07BFB074-DC9A-41F5-9A7E-A3569CD4299A", "Identifiers must be unique.");
			foreach (var claimMapping in proposedValue.ClaimsMappings.Cast<OIDCClaimsMapping>())
			{
				claimMapping.ValidateClaimName();
				if (claimMapping.ClaimNameInfo.HasErrors())
				{
					throw new RegistryValidationException(claimMapping.ClaimNameInfo.GetErrors().First().Message);
				}

				claimMapping.ValidateIdentifier();
				if (claimMapping.IdentifierInfo.HasErrors())
				{
					throw new RegistryValidationException(claimMapping.IdentifierInfo.GetErrors().First().Message);
				}

				if (seenClaims.Contains(claimMapping.ClaimName))
				{
					claimMapping.ClaimNameInfo.AddError(claimNameMustBeUniqueError);
					throw new RegistryValidationException(claimNameMustBeUniqueError);
				}

				if (seenIdentifiers.Contains(claimMapping.Identifier))
				{
					claimMapping.IdentifierInfo.AddError(claimNameMustBeUniqueError);
					throw new RegistryValidationException(identifierMustBeUniqueError);
				}

				seenClaims.Add(claimMapping.ClaimName);
				seenIdentifiers.Add(claimMapping.Identifier);
			}

			var seenScopes = new HashSet<string>();
			var scopesMustBeUniqueError = Res.GetString("1E98AABC-3AD0-45DD-B683-716DF515E01B", "Scopes must be unique.");
			foreach (var scope in proposedValue.Scopes.Cast<OIDCScope>())
			{
				scope.ValidateScopeName();
				if (scope.ScopeNameInfo.HasErrors())
				{
					throw new RegistryValidationException(scope.ScopeNameInfo.GetErrors().First().Message);
				}

				if (seenScopes.Contains(scope.ScopeName))
				{
					scope.ScopeNameInfo.AddError(scopesMustBeUniqueError);
					throw new RegistryValidationException(scopesMustBeUniqueError);
				}

				seenScopes.Add(scope.ScopeName);
			}

			if (proposedValue.IsOIDCEnabled && !proposedValue.IsVerified)
			{
				throw new RegistryValidationException(notVerifiedMessage);
			}
		}

		protected override IRegistryEditorInfo GetNewDefaultEditorInfo()
		{
			return new OIDCConfigRegistryEditorInfo();
		}

		protected override bool HasDefaultEditorInfoCore => true;
		protected override bool IsValidatedOnSetEvenIfEqualDefaultValueCore => false;
	}

	[RegistryEditor(OIDCConfigRegistryEditorInfo.FullyQualifiedEditorClassAndAssembly)]
	public class OIDCConfigRegistryEditorInfo : RegistryEditorInfo
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Fully qualified type name")]
		internal const string FullyQualifiedEditorClassAndAssembly = "Enterprise.Registry.GUI.OIDCRegistryItemEditor, Enterprise.Registry.GUI";

		public override Type BaseDataTypeToBeEdited
		{
			get { return typeof(OIDCConfigRegistryDataType); }
		}
	}
}
