using System;
using System.Globalization;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MailManager.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MailManager.MessageProcessor
{
	class ProcessMessageMethod<T>
	{
		public ProcessMessageMethod(MethodInfo method)
		{
			_methodHandle = method.MethodHandle;
			_typeHandle = method.ReflectedType.TypeHandle;
			_conditions = (MessageFilterConditionAttribute[])method.GetCustomAttributes(typeof(MessageFilterConditionAttribute), false);
		}

		public bool IsMatch(IMessageProcessorContext context, T bo)
		{
			if (_conditions.Length > 0)
			{
				try
				{
					return Array.TrueForAll(_conditions, condition => IsMatch(condition, bo));
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					HandleException(context, bo, ex);
				}
			}

			return false;
		}

		public bool Process(IMessageProcessorContext context, T bo)
		{
			try
			{
				return Delegate(context, bo);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				HandleException(context, bo, ex);
				return false;
			}
		}

		bool IsMatch(MessageFilterConditionAttribute condition, T bizObj)
			=> condition.IsMatch(GetPropertyValue(bizObj, condition.PropertyName));

		object GetPropertyValue(object source, string datamember)
		{
			foreach (var member in datamember.Split('.'))
			{
				if (source == null)
				{
					break;
				}

				source = GetSinglePropertyValue(source, member);
			}

			return source;
		}

		object GetSinglePropertyValue(object source, string member)
		{
			try
			{
				return source.GetType().InvokeMember(member, BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.Public, null, source, null, CultureInfo.CurrentCulture);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				throw new ArgumentException("Can't retrieve property " + member + " from " + source.GetType().FullName, ex);
			}
		}

		void HandleException(IMessageProcessorContext context, T bo, Exception ex)
		{
			var bizObj = bo as BusinessObject;
			var pk = bizObj == null ? (NoResString)"None" : bizObj.PK.ToString();
			var message = string.Format((NoResString)"An unhandled exception occured during processing of {0}(PK={{{1}}}). Exception details follow:", typeof(T).Name, pk);
			context.Logger.Log(LogType.Error, message, ex);

			var stackTraceHash = System.Security.Cryptography.MD5.Create().ComputeHash(Encoding.ASCII.GetBytes(ex.StackTrace));
			var sb = new StringBuilder();
			Array.ForEach(stackTraceHash, b => sb.Append(b.ToString("X2")));
			ErrorReporter.ReportOnce(sb.ToString(), ex.Message, ex);
		}

		#region Delegate

		delegate bool ProcessMessageDelegate(IMessageProcessorContext context, T bo);

		ProcessMessageDelegate Delegate
		{
			get
			{
				if (_delegate == null)
				{
					var methodInfo = (MethodInfo)MethodInfo.GetMethodFromHandle(_methodHandle, _typeHandle);
					var dm = new DynamicMethod("GeneratedProcessMessageDelegate", typeof(bool), new Type[] { typeof(IMessageProcessorContext), typeof(T) }, GetType(), true);

					var ilg = dm.GetILGenerator();

					// filterInstance = context.GetFilterInstance<FilterInstanceType>()
					ilg.Emit(OpCodes.Ldarg_0);
					var getFilterInstanceInfo = typeof(IMessageProcessorContext).GetMethod("GetFilterInstance").MakeGenericMethod(methodInfo.ReflectedType);
					ilg.Emit(OpCodes.Callvirt, getFilterInstanceInfo);

					// return &filterInstance.FilterMethod(t.PK or t or context.Logger)
					foreach (var parameterInfo in methodInfo.GetParameters())
					{
						var isGuid = parameterInfo.ParameterType.Equals(typeof(Guid));
						var isZGuid = parameterInfo.ParameterType.Equals(typeof(ZGuid));
						if (isGuid || isZGuid)
						{
							ilg.Emit(OpCodes.Ldarg_1);
							var getPKInfo = typeof(BusinessObject).GetProperty("PK").GetGetMethod();
							ilg.Emit(OpCodes.Callvirt, getPKInfo);
							if (isGuid)
							{
								ilg.DeclareLocal(typeof(ZGuid));
								ilg.Emit(OpCodes.Stloc_0);
								ilg.Emit(OpCodes.Ldloca_S, (byte)0);
								var toGuidInfo = typeof(ZGuid).GetMethod("ToGuid");
								ilg.Emit(OpCodes.Call, toGuidInfo);
							}
						}
						else if (parameterInfo.ParameterType.IsAssignableFrom(typeof(T)))
						{
							ilg.Emit(OpCodes.Ldarg_1);
						}
						else if (parameterInfo.ParameterType.IsAssignableFrom(typeof(ILogger)))
						{
							ilg.Emit(OpCodes.Ldarg_0);
							var getLoggerInfo = typeof(IMessageProcessorContext).GetProperty("Logger").GetGetMethod();
							ilg.Emit(OpCodes.Callvirt, getLoggerInfo);
						}
						else
						{
							var message = string.Format((NoResString)"Invalid '{0} {1}' parameter type for '{2}.{3}' filter method.",
								parameterInfo.ParameterType.Name,
								parameterInfo.Name,
								methodInfo.DeclaringType.Name,
								methodInfo.Name);
							throw new NotSupportedException(message);
						}
					}

					ilg.Emit(OpCodes.Call, methodInfo);
					ilg.Emit(OpCodes.Ret);

					_delegate = (ProcessMessageDelegate)dm.CreateDelegate(typeof(ProcessMessageDelegate));
				}

				return _delegate;
			}
		}

		ProcessMessageDelegate _delegate;

		#endregion

		RuntimeMethodHandle _methodHandle;
		RuntimeTypeHandle _typeHandle;
		readonly MessageFilterConditionAttribute[] _conditions;
	}
}

