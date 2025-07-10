using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Dash.Integration;
using Enterprise.Dash.Integration.Services;
using Enterprise.DocumentScanning.Integration;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using WTG.Shared.Dash.Common;
using WTG.Shared.Dash.UniversalMessaging.Serialization;
using static Enterprise.Integration.Customs;
using SharedConstants = WTG.Shared.Dash.Common.Constants;
using UniversalMessagingConstants = WTG.Shared.Dash.UniversalMessaging.Constants;

namespace Enterprise.Dash.Business.Services
{
	public class DashCompletionService : IDashCompletionService
	{
		readonly IShipamaxService shipamaxService;
		readonly IDashPostingService dashPostingService;

		public DashCompletionService(IShipamaxService shipamaxService, IDashPostingService dashPostingService)
		{
			this.shipamaxService = shipamaxService;
			this.dashPostingService = dashPostingService;
		}

		public void Complete(Guid dashDocumentId, BusinessObjectFactory factory = null)
		{
			factory ??= new BusinessObjectFactory();
			var dashDocument = factory.Load<DashDocument>(dashDocumentId)
				?? throw new DashException($"Can't find DashDocument record matching PK {dashDocumentId}.");

			Complete(dashDocument, factory);
		}

		public void Complete(DashDocument dashDocument, BusinessObjectFactory factory = null)
		{
			factory ??= new BusinessObjectFactory();
			ValidateDocument(dashDocument);

			var dashCommercialInvoice = factory.LoadTop1<DashCommercialInvoice>(new ZQuery(DashCommercialInvoiceSchema.DCI_DDD_DashDocID, dashDocument.PK))
				?? throw new DashException($"Can't find DashCommercialInvoice record matching DashDocument PK {dashDocument.PK}");

			CompleteCommercialInvoice(factory, dashDocument, dashCommercialInvoice);
		}

		void ValidateDocument(DashDocument dashDocument)
		{
			if (dashDocument.DDD_IsObsolete)
			{
				throw new DashException($"DashDocument with PK {dashDocument.PK} is obsolete.");
			}
			else if (dashDocument.DDD_ParseStatus != SharedConstants.ParseStatus.Code.SubmittedForCompletion)
			{
				throw new DashException($"DashDocument with PK {dashDocument.PK} must have \"{SharedConstants.ParseStatus.Description.SubmittedForCompletion}\" status.");
			}
			else if (dashDocument.DDD_ParseType != SharedConstants.ParseType.Code.CommercialInvoice)
			{
				throw new DashException($"Currently only documents with {SharedConstants.ParseType.Code.CommercialInvoice} parse types support completion with UXML posting.");
			}
		}

		void CompleteCommercialInvoice(BusinessObjectFactory factory, DashDocument dashDocument, DashCommercialInvoice dashCommercialInvoice)
		{
			var registrationKey = ObjectFactory.Get<IProductRegistration>().Key;

			var xml = dashCommercialInvoice
				.ToEntityCommercialInvoice()
				.ToUniversalShipment(registrationKey.EnterpriseCode, registrationKey.ServerCode, GlbBranch.CurrentBranch.Company.GC_Code, GetJobInfos(factory, dashDocument).ToArray())
				.ToXml()
				.ToString();

			var parseResult = new ShipamaxParseResult
			{
				ParseStatus = ShipamaxParseStatus.Complete,
				XmlParseResult = xml,
			};

			try
			{
				shipamaxService.SaveParseResult(dashDocument.DDD_DocID.ToGuid(), dashDocument.DDD_DocToken, parseResult);
			}
			catch (ShipamaxServiceException ex)
			{
				throw new DashException(ex.Message);
			}

			dashPostingService.PostUxml(
				GlbBranch.CurrentBranch.PK,
				GlbDepartment.CurrentDepartment.PK,
				dashDocument.PK,
				xml,
				factory);

			dashDocument.DDD_ParseStatus = SharedConstants.ParseStatus.Code.Complete;
			factory.Save();
		}

		List<(string jobType, string jobReference)> GetJobInfos(BusinessObjectFactory factory, DashDocument dashDocument)
		{
			var jobInfos = new List<(string JobType, string JobReference)>();

			if (dashDocument.DDD_RelatedEntityTableCode == SharedConstants.RelatedEntityType.TableCode.Shipment)
			{
				jobInfos.Add((UniversalMessagingConstants.DataContextType.ForwardingShipment, dashDocument.DDD_RelatedEntityRef));

				AddJobDeclarationTarget(factory, new ZQuery(JobDeclarationSchema.JE_JS, dashDocument.DDD_RelatedEntityID), jobInfos);
			}
			else if (dashDocument.DDD_RelatedEntityTableCode == SharedConstants.RelatedEntityType.TableCode.Declaration)
			{
				jobInfos.Add((UniversalMessagingConstants.DataContextType.CustomsDeclaration, dashDocument.DDD_RelatedEntityRef));
			}

			return jobInfos;
		}

		void AddJobDeclarationTarget(BusinessObjectFactory factory, ZQuery zQuery, List<(string JobType, string JobReference)> jobInfos)
		{
			var jobDeclaration = factory.LoadTop1<IBaseJobDeclaration>(zQuery);

			if (jobDeclaration != null)
			{
				jobInfos.Add((UniversalMessagingConstants.DataContextType.CustomsDeclaration, jobDeclaration.JE_DeclarationReference));
			}
		}
	}
}
