using System;
using CargoWise.Application;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.DocumentVisualizer.Business
{
	public sealed class RoutingRuleValidator : IRoutingRuleValidator
	{
		public RoutingRuleValidator()
			: this(new EHubRoutingRuleValidatorService())
		{
		}

		public RoutingRuleValidator(IRoutingRuleValidatorService service)
		{
			_ = service ?? throw new ArgumentNullException(nameof(service));
			this.service = service;
		}

		readonly IRoutingRuleValidatorService service;

		public bool IsValid(string interchange)
		{
			return IsValidWithRecipientIdsRetrieved(interchange).ValidationResult;
		}

		public (bool ValidationResult, string[] RecipientIds) IsValidWithRecipientIdsRetrieved(string interchange)
		{
			if (string.IsNullOrWhiteSpace(interchange))
			{
				return (false, null);
			}

			var request = CreateRequest(interchange);
			var response = service.PerformValidationCheck(request);

			return (ValidationResult: response.ValidationResult ?? true,
							RecipientIds: response.RecipientIds ?? Array.Empty<string>());
		}

		RoutingRuleValidatorRequest CreateRequest(string interchange)
		{
			if (FreightDataRegistry.Instance.EnableCarrierMessagingConnectionValidationTestUrl.Value)
			{
				return new RoutingRuleValidatorRequest
				{
					ServiceUrl = "https://ehub-routingws-test.wisegrid.net/RoutingRuleValidationWebService.svc",
					ClientId = "HYETSTTST",
					Password = (NoResString)"test",
					Interchange = interchange
				};
			}

			return new RoutingRuleValidatorRequest
			{
				ServiceUrl = "https://ehub-routingws.wisegrid.net/RoutingRuleValidationWebService.svc",
				ClientId = GlbCompany.CurrentCompany.GetLicenceCode(),
				Password = ObjectFactory.Get<IProductRegistration>()?.Key?.Password,
				Interchange = interchange
			};
		}
	}
}
