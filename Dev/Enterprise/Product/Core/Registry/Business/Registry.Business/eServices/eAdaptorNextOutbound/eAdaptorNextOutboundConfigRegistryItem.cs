using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public class eAdaptorNextOutboundConfigRegistryItem : StronglyTypedRegistryItem<eAdaptorNextOutboundConfig>
	{
		public eAdaptorNextOutboundConfigRegistryItem(
			string name,
			MultilingualString category,
			MultilingualString caption,
			MultilingualString hint,
			RegistryStorageFlags storage,
			RegistryOptions options,
			eAdaptorNextOutboundConfig defaultValue)
		: base(new RegistryItemImpl(name, category, caption, hint, new eAdaptorNextOutboundConfigRegistryDataType(eAdaptorNextOutboundConfig.DefaultValue), storage, options, defaultValue))
		{
		}
	}

	[RegistryEditor(eAdaptorNextOutboundConfigRegistryEditorInfo.FullyQualifiedEditorClassAndAssembly)]
	public class eAdaptorNextOutboundConfigRegistryDataType : NonPersistentBusinessObjectRegistryDataType<eAdaptorNextOutboundConfig>
	{
		public eAdaptorNextOutboundConfigRegistryDataType(eAdaptorNextOutboundConfig defaultValue)
			: base(defaultValue)
		{
		}
		protected override eAdaptorNextOutboundConfig CloneValue(eAdaptorNextOutboundConfig value)
		{
			return value.Clone();
		}

		protected override void ValidateCore(
			IRegistryItem registryItem,
			eAdaptorNextOutboundConfig proposedValue,
			Guid companyPK,
			Guid branchPK,
			Guid departmentPK)
		{
			base.ValidateCore(registryItem, proposedValue, companyPK, branchPK, departmentPK);

			proposedValue.ClearAllNotifications();

			proposedValue.ValidateAuthorizationGrantTypeCode();
			if (proposedValue.AuthorizationGrantTypeCodeInfo.HasErrors())
			{
				throw new RegistryValidationException(proposedValue.AuthorizationGrantTypeCodeInfo.GetErrors().First().Message);
			}

			proposedValue.ValidateAuthorizationURL();
			if (proposedValue.AuthorizationURLInfo.HasErrors())
			{
				throw new RegistryValidationException(proposedValue.AuthorizationURLInfo.GetErrors().First().Message);
			}

			var seenScopes = new HashSet<string>();
			foreach (var scope in proposedValue.Scopes.Cast<OAuth2Scope>())
			{
				scope.ValidateScopeName();
				if (scope.ScopeNameInfo.HasErrors())
				{
					throw new RegistryValidationException(scope.ScopeNameInfo.GetErrors().First().Message);
				}

				if (seenScopes.Contains(scope.ScopeName))
				{
					var scopesMustBeUniqueError = Res.GetString("1b8c2281-8286-441d-8b21-11844469f659", "Scopes must be unique.");
					scope.ScopeNameInfo.AddError(scopesMustBeUniqueError);
					throw new RegistryValidationException(scopesMustBeUniqueError);
				}

				seenScopes.Add(scope.ScopeName);
			}

			proposedValue.ValidateIsVerified();
			if (proposedValue.IsVerifiedInfo.HasErrors())
			{
				throw new RegistryValidationException(proposedValue.IsVerifiedInfo.GetErrors().First().Message);
			}
		}

		protected override IRegistryEditorInfo GetNewDefaultEditorInfo()
		{
			return new eAdaptorNextOutboundConfigRegistryEditorInfo();
		}

		protected override bool HasDefaultEditorInfoCore => true;
		protected override bool IsValidatedOnSetEvenIfEqualDefaultValueCore => false;
	}

	[RegistryEditor(eAdaptorNextOutboundConfigRegistryEditorInfo.FullyQualifiedEditorClassAndAssembly)]
	public class eAdaptorNextOutboundConfigRegistryEditorInfo : RegistryEditorInfo
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "const string")]
		internal const string FullyQualifiedEditorClassAndAssembly = "Enterprise.Registry.GUI.eAdaptorNextOutboundRegistryItemEditor, Enterprise.Registry.GUI";

		public override Type BaseDataTypeToBeEdited
		{
			get { return typeof(eAdaptorNextOutboundConfigRegistryDataType); }
		}
	}
}
