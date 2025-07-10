using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Business
{
	public static class ActionMethodsHelper
	{
		public static object TargetReference(BusinessObject target)
		{
			if (target == null)
			{
				throw new ArgumentNullException(nameof(target));
			}

			var genericJob = target.Factory.LoadGenericJob<GenericJob.GenericJob>(target.PK, target.TablePrefix);
			ControllerID controller = genericJob != null ? genericJob.GetConsumerController() : null;

			if (genericJob != null && controller != null)
			{
				return new LogControllerLink(target.HumanReadableName, controller, target.PK);
			}
			else
			{
				return target.HumanReadableName;
			}
		}

		public static object ExRateProviderReference(IExchangeRateSource provider)
		{
			if (provider == null)
			{
				throw new ArgumentNullException(nameof(provider));
			}

			ControllerID controller;
			ZGuid pk;

			if ((controller = provider.SourceController) != null && !(pk = provider.SourcePK).IsEmpty)
			{
				return new LogControllerLink(provider.Description, controller, pk);
			}
			else
			{
				return provider.Description;
			}
		}
	}
}

