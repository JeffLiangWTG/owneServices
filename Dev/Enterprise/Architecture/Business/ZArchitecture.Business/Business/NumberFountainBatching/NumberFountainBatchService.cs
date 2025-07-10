using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ZArchitecture.Business
{
	class NumberFountainBatchServiceBuilder : IOnSavingServiceBuilder
	{
		public IEnumerable<IOnSavingService> Build(IEnumerable<BusinessObject> bizos)
		{
			Dictionary<INumberFountainProxy, INumberFountainBatcher> services = null;
			Dictionary<BusinessObject, INumberFountainProxy> bizoFountainLookup = null;

			foreach (var bizo in bizos)
			{
				if (NeedsFountainID(bizo))
				{
					var fountainProxy = GetFountainProxy(bizo);
					if (services == null)
					{
						services = new Dictionary<INumberFountainProxy, INumberFountainBatcher>();
					}
					if (bizoFountainLookup == null)
					{
						bizoFountainLookup = new Dictionary<BusinessObject, INumberFountainProxy>();
					}

					bizoFountainLookup.Add(bizo, fountainProxy);

					if (!services.TryGetValue(fountainProxy, out var batcher))
					{
						services[fountainProxy] = batcher = (bizo is ICustomizableNumberFountainConsumer)
							? new CustomisableNumberFountainBatcher(fountainProxy)
							: new StandardNumberFountainBatcher(fountainProxy);
					}
					batcher.Increment();
				}
			}

			if (services != null)
			{
				yield return new NumberFountainBatchService(services, bizoFountainLookup);
			}
		}

		internal static bool NeedsFountainID(BusinessObject bizo) => bizo is INumberFountainEntityWithID entity && !bizo.IsInDatabase && !bizo.IsDeleted && entity.ID.IsEmpty;

		static INumberFountainProxy GetFountainProxy(BusinessObject bizo)
		{
			if (bizo is INumberFountainConsumer consumer)
			{
				return consumer.Fountain;
			}
			else if (bizo is ICustomizableNumberFountainConsumer customizableConsumer)
			{
				return new CustomisableFountainProxy(customizableConsumer.NumberFormatter.GetFountainProxy());
			}
			else
			{
				throw new InvalidOperationException($"Unable to find a Number Fountain to use in the Batching Service. Type with failure: {bizo.GetType().Name}.");
			}
		}

		#region Impl

		class NumberFountainBatchService : IOnSavingService
		{
			public NumberFountainBatchService(Dictionary<INumberFountainProxy, INumberFountainBatcher> services, Dictionary<BusinessObject, INumberFountainProxy> bizoFountainLookup)
			{
				this.services = services;
				this.bizoFountainLookup = bizoFountainLookup;
			}

			readonly Dictionary<INumberFountainProxy, INumberFountainBatcher> services;
			readonly Dictionary<BusinessObject, INumberFountainProxy> bizoFountainLookup;
			readonly List<INumberFountainEntityWithID> itemsChanged = new List<INumberFountainEntityWithID>();

			public void Apply(IEnumerable<BusinessObject> bizos)
			{
				foreach (var bizo in bizos)
				{
					if (NeedsFountainID(bizo) && services.TryGetValue(bizoFountainLookup[bizo], out var batcher))
					{
						var entity = (INumberFountainEntityWithID)bizo;
						entity.ID = batcher.GetNext(entity);
						itemsChanged.Add(entity);
					}
				}
			}

			public void OnSaveFailed()
			{
				foreach (var item in itemsChanged.Where(i => i is BusinessObject biz && !biz.IsDeleted))
				{
					item.ID = ZString.Empty;
				}
			}
		}

		interface INumberFountainBatcher
		{
			void Increment();

			string GetNext(INumberFountainEntityWithID entity);
		}

		abstract class NumberFountainBatcher<T> : INumberFountainBatcher
		{
			public NumberFountainBatcher(INumberFountainProxy fountain)
			{
				this.fountain = fountain;
			}

			int idsNeeded;
			readonly INumberFountainProxy fountain;
			readonly Queue<T> ids = new Queue<T>();

			public void Increment() => idsNeeded++;

			public string GetNext(INumberFountainEntityWithID entity)
			{
				string next;
				if (ids.Count > 0)
				{
					next = GetNextNumber(entity, fountain, ids.Dequeue());
				}
				else if (idsNeeded > 0)
				{
					PopulateIds(entity.Factory);
					next = GetNextNumber(entity, fountain, ids.Dequeue());
				}
				else
				{
					ErrorReporter.ReportOnce("BatchNumberFountainService" + fountain.ToString(), "Getting Id without batching...");
					next = GetNextNumber(entity, fountain, GetNext(entity.Factory, fountain));
				}
				return next;
			}

			void PopulateIds(IDbConnected connected)
			{
				foreach (var n in GetNexts(connected, fountain, idsNeeded))
				{
					ids.Enqueue(n);
				}
				idsNeeded = 0;
			}

			protected abstract T GetNext(IDbConnected connected, INumberFountainProxy fountain);
			protected abstract T[] GetNexts(IDbConnected connected, INumberFountainProxy fountain, int idsNeeded);
			protected abstract string GetNextNumber(INumberFountainEntityWithID entity, INumberFountainProxy fountain, T id);
		}

		class StandardNumberFountainBatcher : NumberFountainBatcher<string>
		{
			public StandardNumberFountainBatcher(INumberFountainProxy fountain)
				: base(fountain)
			{
			}

			protected override string GetNext(IDbConnected connected, INumberFountainProxy fountain) => fountain.GetNextFormatted(connected);

			protected override string GetNextNumber(INumberFountainEntityWithID entity, INumberFountainProxy fountain, string id) => id;

			protected override string[] GetNexts(IDbConnected connected, INumberFountainProxy fountain, int idsNeeded) => fountain.GetNextsFormatted(connected, idsNeeded);
		}

		class CustomisableNumberFountainBatcher : NumberFountainBatcher<long>
		{
			public CustomisableNumberFountainBatcher(INumberFountainProxy fountain)
				: base(fountain)
			{
			}

			protected override long GetNext(IDbConnected connected, INumberFountainProxy fountain) => fountain.GetNext(connected);

			protected override string GetNextNumber(INumberFountainEntityWithID entity, INumberFountainProxy fountain, long id) => ((ICustomizableNumberFountainConsumer)entity).NumberFormatter.GetFormattedNumber(id, fountain);

			protected override long[] GetNexts(IDbConnected connected, INumberFountainProxy fountain, int idsNeeded) => fountain.GetNexts(connected, idsNeeded);
		}

		#endregion
	}
}
