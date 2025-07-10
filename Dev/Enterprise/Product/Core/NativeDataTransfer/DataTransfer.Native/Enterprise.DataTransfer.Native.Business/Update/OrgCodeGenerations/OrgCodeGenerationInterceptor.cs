using System;
using CargoWise.EntityFramework;
using CargoWise.Organizations.CodeGeneration;
using Enterprise.DataTransfer.Native.Business.Update.CodeMappings;
using Enterprise.DataTransfer.Native.Common;
using Enterprise.DataTransfer.Native.Common.CodeMappings;
using Enterprise.DataTransfer.Native.Common.Converters;
using Enterprise.DataTransfer.Native.Common.Interceptors;
using Enterprise.DataTransfer.Native.Utils;

namespace Enterprise.DataTransfer.Native.Business.Update.OrgCodeGenerations
{
	public class OrgCodeGenerationInterceptor : BaseInterceptor
	{
		public OrgCodeGenerationInterceptor(OrgCodeGenerationSetting setting, AncillaryImportServices sessionServices)
			: base(setting, sessionServices)
		{
			this.setting = setting ?? throw new ArgumentException(FormattableString.Invariant($"{nameof(OrgCodeGenerationSetting)} was null"));

			var objectFactory = setting.Context.ObjectFactory;
			var connection = setting.Context.Connection;

			codeInfoConverter = new OrgCodeInfoConverter(connection, sessionServices);
			codeGenerator = new NativeOrgCodeGenerator();
			codeGeneratorFactory = new BusinessObjectFactory(connection);

			codeMappingRepository = new CodeMappingRepository(objectFactory);
		}
		readonly BusinessObjectFactory codeGeneratorFactory;
		readonly OrgCodeGenerationSetting setting;

		public override void Invoke(IEntitySet entitySet)
		{
			if (DisableSearchFromCodeMapping || !CodeMappingInterception(setting, entitySet))
			{
				OrgCodeGenerationInterception(setting, entitySet);
			}
		}

		bool DisableSearchFromCodeMapping
		{
			get { return setting.Context.AlwaysUseInternalPK; }
		}

		bool EnableGenerateCodeMapping
		{
			get { return !setting.Context.AlwaysUseInternalPK && setting.EnableCodeMapping; }
		}

		/// <summary>
		/// Try to find Code Mapping
		/// Do not Generate Organization Code, if Code Mapping existed
		/// </summary>
		/// <param name="setting"></param>
		/// <param name="entitySet"></param>
		/// <returns></returns>
		bool CodeMappingInterception(OrgCodeGenerationSetting setting, IEntitySet entitySet)
		{
			// Might be time consuming here
			var org = entitySet.Root;
			var localCode = FindLocalCode(org, setting.OwnerCode);
			if (localCode.IsEmpty())
			{
				return false;
			}

			if (!sessionServices.OrganisationLocalToForeignCodeMappings.ContainsKey(localCode))
			{
				sessionServices.OrganisationLocalToForeignCodeMappings.Add(localCode, (string)org[LocalCodePropertyName]);
			}
			org[LocalCodePropertyName] = localCode;
			Function(entitySet);
			return true;
		}

		void OrgCodeGenerationInterception(OrgCodeGenerationSetting setting, IEntitySet entitySet)
		{
			// Create a new BusinessObjectFactory to find owner code
			// Might be time consuming here
			var ownerCode = setting.OwnerCode;

			var entity = entitySet.Root;
			if (entity.Action != EntityAction.INSERT && entity.Action != EntityAction.MERGE)
			{
				Function(entitySet);
				return;
			}

			var generatedCode = GenerateCode(entity);
			if (generatedCode.IsEmpty())
			{
				Function(entitySet);
				return;
			}

			var foreignCode = GetForeignCode(entity);

			entity[LocalCodePropertyName] = generatedCode;

			CodeMapping codeMapping = null;
			if (EnableGenerateCodeMapping && !foreignCode.IsEmpty())
			{
				codeMapping = EDICodeMapper.FindGlobalCodeMapping(entity.TableName, LocalCodePropertyName);
				codeMapping.LocalCode = generatedCode;
				codeMapping.ForeignCode = foreignCode;
				codeMapping.OwnerCode = ownerCode;

				sessionServices.OrganisationLocalToForeignCodeMappings[generatedCode] = foreignCode;
			}

			Function(entitySet);

			if (codeMapping != null)
			{
				codeMapping.LocalGuid = entity.InternalPK;
				codeMappingRepository.CreateCodeMapping(codeMapping);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "const string")]
		const string LocalCodePropertyName = "Code";

		string FindLocalCode(IEntity org, string ownerCode)
		{
			var localCode = string.Empty;
			var foreignCode = GetForeignCode(org);
			if (!foreignCode.IsEmpty())
			{
				var codeMapping = EDICodeMapper.FindGlobalCodeMapping(org.TableName, LocalCodePropertyName);
				if (codeMapping != null)
				{
					localCode = codeMappingRepository.MapLocalCode(codeMapping.Relationship, foreignCode, ownerCode);

					if (!string.IsNullOrEmpty(localCode))
					{
						if (!sessionServices.EntitiesReferencingPK.TryGetValue(Guid.Empty, out var collectionUsedToStoreCodeMappingDetailsWithoutRelyingOnAStupidStatic))
						{
							collectionUsedToStoreCodeMappingDetailsWithoutRelyingOnAStupidStatic = new EntityCollection();
							sessionServices.EntitiesReferencingPK.Add(Guid.Empty, collectionUsedToStoreCodeMappingDetailsWithoutRelyingOnAStupidStatic);
						}
					}
				}
			}
			return localCode;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "const string")]
		const string ForeignCodePropertyName = "Code";

		string GetForeignCode(IEntity entity)
		{
			return entity.HasProperty(ForeignCodePropertyName)
				? (string)entity[ForeignCodePropertyName]
				: string.Empty;
		}

		string GenerateCode(IEntity org)
		{
			var info = codeInfoConverter.Convert(org, setting.Context);
			var code = codeGenerator.Generate(info, codeGeneratorFactory);
			return code;
		}

		#region Dependency

#if DEBUG
		internal IOrgCodeGenerator CodeGenerator
		{
			get { return codeGenerator; }
			set { codeGenerator = value; }
		}

		internal IEntityConverter<IOrgCodeInfo> CodeInfoConverter
		{
			get { return codeInfoConverter; }
			set { codeInfoConverter = value; }
		}

		internal ICodeMappingHelper CodeMappingRepository
		{
			get { return codeMappingRepository; }
			set { codeMappingRepository = value; }
		}
#endif
		#endregion

		IOrgCodeGenerator codeGenerator;
		IEntityConverter<IOrgCodeInfo> codeInfoConverter;
		ICodeMappingHelper codeMappingRepository;
	}
}
