using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EventReference;
using CargoWise.Macros;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.Business
{
	sealed class EventDataModel : IEventDataModel
	{
		public EventDataModel(StmALog log)
		{
			this.log = Argument.NotNull(log, "log");
		}

		readonly StmALog log;

		string IEventDataModel.Event
		{
			get { return log.SL_SE_NKEvent; }
		}

		string IEventDataModel.EventDescription
		{
			get { return log.SL_EventDescription; }
		}

		string IEventDataModel.Reference
		{
			get { return log.SL_Reference; }
		}

		string IEventDataModel.ReferenceFreeText
		{
			get { return log.ReferenceFreeText; }
		}

		IReadOnlyDictionary<string, string> IEventDataModel.Parameters
		{
			get { return modelParameters ?? (modelParameters = new ReadOnlyDictionaryWrapper<string, string>(() => log.Parameters)); }
		}

		IReadOnlyDictionary<string, string> modelParameters;

		IEnumerable<IMacroLibrary> IEventDataModel.Libraries
		{
			get
			{
				yield return new StandardLibrary();
				yield return new DescriptionMacroLibrary(log.Factory);
			}
		}

		string IEventDataModel.GetCityCountry(string unloco)
		{
			return UnlocoHelper.GetCityCountry(unloco);
		}

		string IEventDataModel.GetEventDescription(string code)
		{
			return log.Factory.GetCachedValue($"GetEventDescription.{code}", () =>
			{
				var stmEvent = log.Factory.LoadTop1<StmEvent>(new ZQuery(StmEventSchema.SE_Code, code));
				if (stmEvent != null)
				{
					return stmEvent.SE_DescMultilingual;
				}
				return string.Empty;
			});
		}

		bool IEventDataModel.IsUnloco(string unloco)
		{
			return UnlocoHelper.IsUnloco(unloco);
		}

		IUnlocoHelper UnlocoHelper
		{
			get { return log.Factory.GetCachedValue("StmALog_UnlocoHelper", () => ObjectFactory.Get<IUnlocoHelper>(nameof(IUnlocoHelper), new object[] { log.Factory })); }
		}
	}
}
