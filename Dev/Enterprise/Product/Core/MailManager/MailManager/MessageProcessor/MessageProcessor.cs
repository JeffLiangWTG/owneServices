using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Enterprise.MailManager.Integration;

namespace Enterprise.MailManager.MessageProcessor
{
	class MessageProcessor<T> : IMessageProcessor<T>
	{
		public MessageProcessor(IMessageFilterConfig[] messageFilterConfigs)
		{
			_messageFilterConfigs = messageFilterConfigs;
		}

		public bool Process(IMessageProcessorContext context, T bo)
		{
			foreach (var method in ProcessMethods)
			{
				if (method.IsMatch(context, bo) && method.Process(context, bo))
				{
					return true;
				}
			}

			return false;
		}

		public bool IsMatch(IMessageProcessorContext context, T bo)
			=> ProcessMethods.Any(method => method.IsMatch(context, bo));

		#region ProcessMethods

		ProcessMessageMethod<T>[] ProcessMethods
		{
			get
			{
				if (_processMethods == null)
				{
					var result = new List<ProcessMessageMethod<T>>();
					Array.ForEach(_messageFilterConfigs, fc => result.AddRange(GetProcessMessageMethods(fc)));
					_processMethods = result.ToArray();
				}

				return _processMethods;
			}
		}

		IEnumerable<ProcessMessageMethod<T>> GetProcessMessageMethods(IMessageFilterConfig filterConfig)
		{
			var result = new List<ProcessMessageMethod<T>>();
			var filterType = Type.GetType(Assembly.CreateQualifiedName(filterConfig.TypeAssemblyName, filterConfig.TypeName), false);
			if (filterType != null)
			{
				foreach (var method in filterType.GetMethods(BindingFlags.Public | BindingFlags.Instance))
				{
					var attributes = method.GetCustomAttributes(typeof(MessageFilterConditionAttribute), false);
					if (attributes.Length > 0)
					{
						result.Add(new ProcessMessageMethod<T>(method));
					}
				}
			}

			return result;
		}

		ProcessMessageMethod<T>[] _processMethods;

		#endregion

		readonly IMessageFilterConfig[] _messageFilterConfigs;
	}
}

