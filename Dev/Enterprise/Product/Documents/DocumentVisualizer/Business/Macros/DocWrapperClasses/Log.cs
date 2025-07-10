using System;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using CargoWise.Common;
using CargoWise.Macros;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Macros;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.DocumentVisualizer.Business
{
	sealed class Log : IMacroObject, IMacroObjectMetaDataProvider<Log>, ILog
	{
		public Log(StmALog log)
		{
			Argument.NotNull(log, nameof(log));
			this.log = log;
		}

		readonly StmALog log;

		#region Properties

		public ZDateTime EventTime => log.SL_EventTime.IsValid ? log.SL_EventTime.ToDateTime() : default(ZDateTime);
		public ZDateTime PostedTime => log.SL_PostedTimeUtc.IsValid ? log.SL_PostedTimeUtc.ToDateTime() : default(ZDateTime);
		public ZString Reference => log.ReferenceFreeText;
		public ZString EventDetails => log.DisplayEventReference;

		public IEvent Event
		{
			get
			{
				if (_event == null
					&& log.Event != null)
				{
					_event = new EventInfo(log.Event);
				}

				return _event;
			}
		}

		IEvent _event;

		public IUser User
		{
			get
			{
				if (user == null
					&& log.User is GlbStaff staff)
				{
					user = new User(staff);
				}

				return user;
			}
		}

		IUser user;

		#endregion

		#region IMacroObject members

		object IMacroObject.Value => this;

		Type IMacroObject.Type => typeof(Log);

		DynamicMetaObject IDynamicMetaObjectProvider.GetMetaObject(Expression parameter)
		{
			if (parameter == null)
			{
				throw new ArgumentNullException(nameof(parameter));
			}

			return new MacroObjectDynamicMetaObject<Log>(parameter, this, this);
		}

		#endregion

		#region IMacroObjectMetaDataProvider<Log> members

		object IMacroObjectMetaDataProvider<Log>.GetProperty(Log macroObject, string name)
		{
			if (!CargoWise.EventReference.Constants.EventReferenceParameters.Codes.All.Contains(name))
			{
				macroObject.ThrowPropertyNotFound(name);
				return null;
			}

			var parameters = macroObject.log.Parameters;
			return parameters != null && parameters.ContainsKey(name)
				? parameters[name]
				: string.Empty;
		}

		object IMacroObjectMetaDataProvider<Log>.InvokeIndexer(Log macroObject, params object[] parameters)
		{
			macroObject.ThrowIndexerNotFound(parameters);
			return null;
		}

		object IMacroObjectMetaDataProvider<Log>.InvokeFunction(Log macroObject, string name, params object[] parameters)
		{
			macroObject.ThrowFunctionNotFound(name, parameters);
			return null;
		}

		#endregion
	}
}