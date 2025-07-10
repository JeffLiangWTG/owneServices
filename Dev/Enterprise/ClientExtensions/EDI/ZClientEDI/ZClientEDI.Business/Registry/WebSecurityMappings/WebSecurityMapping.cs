using System;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Client.EDI.Registry.Business
{
	[XmlSerializerAssembly("ZClientEDI.Business.XmlSerializers")]
	public class WebSecurityMapping : AutoWebSecurityMapping
	{
		public WebSecurityMapping()
		{
			CachedModuleMappingList = new Lazy<CodeDescriptionPairList>(() => Lookups.ModuleMappingList);
		}

		#region Properties

		[ThreadSafe] // Property used only to display a value in the grid
		public ZString ModuleMappingDescription
		{
			get { return CachedModuleMappingList.Value.GetDescriptionFromCode(ModuleMapping); }
		}

		[ThreadSafe] // Property used only to display a value in the grid
		public ZString ProductMappingDescription
		{
			get { return Lookups.ProductMappingList.GetDescriptionFromCode(ProductMapping); }
		}

		Lazy<CodeDescriptionPairList> CachedModuleMappingList;

		[List("Lookups.ModuleMappingList")]
		public override ZString ModuleMapping
		{
			get { return base.ModuleMapping; }
			set { base.ModuleMapping = value; }
		}

		[List("Lookups.ProductMappingList")]
		public override ZString ProductMapping
		{
			get { return base.ProductMapping; }
			set
			{
				base.ProductMapping = value;
				CachedModuleMappingList = new Lazy<CodeDescriptionPairList>(() => Lookups.ModuleMappingList);
			}
		}

		[List("Lookups.WebSecurityList")]
		public override ZString WebSecurity
		{
			get { return base.WebSecurity; }
			set { base.WebSecurity = value; }
		}

		#endregion

		#region Validation

		public override void ValidateWebSecurity()
		{
			base.ValidateWebSecurity();
			MandatoryValidation.CheckEntered(WebSecurityInfo);
			ListValidation.ErrorIfInvalidCode(WebSecurityInfo);
		}

		public override void ValidateModuleMapping()
		{
			base.ValidateModuleMapping();
			MandatoryValidation.CheckEntered(ModuleMappingInfo);
			ListValidation.ErrorIfInvalidCode(ModuleMappingInfo);
		}

		public override void ValidateProductMapping()
		{
			base.ValidateProductMapping();
			MandatoryValidation.CheckEntered(ProductMappingInfo);
			ListValidation.ErrorIfInvalidCode(ProductMappingInfo);
		}

		#endregion

		#region Implementation

		public WebSecurityMappingLookups Lookups
		{
			get
			{
				if (lookups == null)
				{
					lookups = new WebSecurityMappingLookups(this);
				}
				return lookups;
			}
		}
		WebSecurityMappingLookups lookups;

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new WebSecurityMapping();
		}

		#endregion
	}
}
