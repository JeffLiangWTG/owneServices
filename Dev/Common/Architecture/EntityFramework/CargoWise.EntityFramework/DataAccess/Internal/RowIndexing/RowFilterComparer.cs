using System;
using System.Data;
using System.Reflection;
using System.Reflection.Emit;
using CargoWise.Common.Testing;
using Enterprise.ZArchitecture.Core;

namespace CargoWise.EntityFramework
{
	public class RowFilterComparer
	{
		#region Static Constructor
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1810:Initialize reference type static fields inline", Justification = "This is a static constructor. Is not applicable")]
		static RowFilterComparer()
		{	
			DynamicMethod cdeDM = new DynamicMethod("CreateDataExpression", dataExpressionType, new Type[] { typeof(DataTable), typeof(string) }, typeof(DataRowViewConverter), true);
			ILGenerator cdeILGen = cdeDM.GetILGenerator();
			cdeILGen.Emit(OpCodes.Ldarg_0);
			cdeILGen.Emit(OpCodes.Ldarg_1);
			cdeILGen.Emit(OpCodes.Newobj, constructorInfo);
			cdeILGen.Emit(OpCodes.Ret);

			createDataExpression = (CreateDataExpressionDelegate)cdeDM.CreateDelegate(typeof(CreateDataExpressionDelegate));

			DynamicMethod invokeDM = new DynamicMethod((NoResString)"Invoke", typeof(bool), new Type[] { typeof(object), typeof(DataRow), typeof(DataRowVersion) }, typeof(DataRowViewConverter), true);
			ILGenerator invokeGen = invokeDM.GetILGenerator();
			invokeGen.Emit(OpCodes.Ldarg_0);
			invokeGen.Emit(OpCodes.Castclass, dataExpressionType);
			invokeGen.Emit(OpCodes.Ldarg_1);
			invokeGen.Emit(OpCodes.Ldarg_2);
			invokeGen.Emit(OpCodes.Call, invokeMethodInfo);
			invokeGen.Emit(OpCodes.Ret);

			invokeDataExpression = (InvokeDataExpressionDelegate)invokeDM.CreateDelegate(typeof(InvokeDataExpressionDelegate));
		}
		#endregion

		public RowFilterComparer(DataTable table, IFilterPart filter)
			: this(table, filter, filter.LiteralTextADO)
		{
		}

		internal RowFilterComparer(DataTable table, IFilterPart filter, string literalText)
		{
			if (table == null)
			{
				throw new ArgumentNullException(nameof(table));
			}
			if (filter == null)
			{
				throw new ArgumentNullException(nameof(filter));
			}
			ZQuery filterAsZQuery = filter as ZQuery;
			if (filterAsZQuery != null && filterAsZQuery.IsNoResultQuery)
			{
				isNoResultQuery = true;
			}
			else
			{
				string whereClause = literalText;
				if (whereClause.Length > 0)
				{
					dataExpression = createDataExpression(table, whereClause);
				}
			}
		}
		readonly object dataExpression;
		readonly bool isNoResultQuery;

		public bool IsMatch(DataRow row)
		{
			return !isNoResultQuery && (dataExpression == null || invokeDataExpression(dataExpression, row, DataRowVersion.Current));
		}

		delegate object CreateDataExpressionDelegate(DataTable table, string filter);
		delegate bool InvokeDataExpressionDelegate(object dataExpression, DataRow row, DataRowVersion version);

		[SuppressThreadStaticFieldMessage]
		static readonly CreateDataExpressionDelegate createDataExpression;
		[SuppressThreadStaticFieldMessage]
		static readonly InvokeDataExpressionDelegate invokeDataExpression;
		[SuppressThreadStaticFieldMessage]
		static readonly Type dataExpressionType = typeof(DataColumn).Assembly.GetType("System.Data.DataExpression");
		[SuppressThreadStaticFieldMessage]
		static readonly MethodInfo invokeMethodInfo = dataExpressionType.GetMethod("Invoke", BindingFlags.Instance | BindingFlags.Public, null, new Type[] { typeof(DataRow), typeof(DataRowVersion) }, null);
		[SuppressThreadStaticFieldMessage]
		static readonly ConstructorInfo constructorInfo = dataExpressionType.GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, null, new Type[] { typeof(DataTable), typeof(string) }, null);
	}
}
