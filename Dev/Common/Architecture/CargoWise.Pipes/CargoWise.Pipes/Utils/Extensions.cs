using System;
using System.Globalization;

namespace CargoWise.Pipes
{
	public static class Extensions
	{
		#region Pipe Adders

		public static IPipe<TOutput> AddSync<TOutput>(this PipeEngine engine, Func<TOutput> func)
		{
			return engine.AddPipe<TOutput>(PipeType.Synchronous, func);
		}

		public static IPipe<TOutput> AddSync<TInput, TOutput>(this PipeEngine engine, Func<TInput, TOutput> func, IPipeDataSource<TInput> dataSource)
		{
			return engine.AddPipe<TOutput>(PipeType.Synchronous, func, dataSource);
		}

		public static IPipe<TOutput> AddSync<TInput1, TInput2, TOutput>(this PipeEngine engine, Func<TInput1, TInput2, TOutput> func, IPipeDataSource<TInput1> dataSource1, IPipeDataSource<TInput2> dataSource2)
		{
			return engine.AddPipe<TOutput>(PipeType.Synchronous, func, dataSource1, dataSource2);
		}

		public static IPipe<TOutput> AddSync<TInput1, TInput2, TInput3, TOutput>(this PipeEngine engine, Func<TInput1, TInput2, TInput3, TOutput> func, IPipeDataSource<TInput1> dataSource1, IPipeDataSource<TInput2> dataSource2, IPipeDataSource<TInput3> dataSource3)
		{
			return engine.AddPipe<TOutput>(PipeType.Synchronous, func, dataSource1, dataSource2, dataSource3);
		}

		public static IPipe<TOutput> AddAsync<TOutput>(this PipeEngine engine, Func<TOutput> func)
		{
			return engine.AddPipe<TOutput>(PipeType.Asynchronous, func);
		}

		public static IPipe<TOutput> AddAsync<TInput, TOutput>(this PipeEngine engine, Func<TInput, TOutput> func, IPipeDataSource<TInput> dataSource)
		{
			return engine.AddPipe<TOutput>(PipeType.Asynchronous, func, dataSource);
		}

		public static IPipe<TOutput> AddAsync<TInput1, TInput2, TOutput>(this PipeEngine engine, Func<TInput1, TInput2, TOutput> func, IPipeDataSource<TInput1> dataSource1, IPipeDataSource<TInput2> dataSource2)
		{
			return engine.AddPipe<TOutput>(PipeType.Asynchronous, func, dataSource1, dataSource2);
		}

		public static IPipe<TOutput> AddAsync<TInput1, TInput2, TInput3, TOutput>(this PipeEngine engine, Func<TInput1, TInput2, TInput3, TOutput> func, IPipeDataSource<TInput1> dataSource1, IPipeDataSource<TInput2> dataSource2, IPipeDataSource<TInput3> dataSource3)
		{
			return engine.AddPipe<TOutput>(PipeType.Asynchronous, func, dataSource1, dataSource2, dataSource3);
		}

		public static void AddPipeEnd<TInput1>(this PipeEngine engine, Action<TInput1> action, IPipeDataSource<TInput1> dataSource1)
		{
			engine.AddPipe<object>(PipeType.Synchronous, action, dataSource1);
		}

		public static void AddPipeEnd<TInput1, TInput2>(this PipeEngine engine, Action<TInput1, TInput2> action, IPipeDataSource<TInput1> dataSource1, IPipeDataSource<TInput2> dataSource2)
		{
			engine.AddPipe<object>(PipeType.Synchronous, action, dataSource1, dataSource2);
		}

		public static void AddPipeEnd<TInput1, TInput2, TInput3>(this PipeEngine engine, Action<TInput1, TInput2, TInput3> action, IPipeDataSource<TInput1> dataSource1, IPipeDataSource<TInput2> dataSource2, IPipeDataSource<TInput3> dataSource3)
		{
			engine.AddPipe<object>(PipeType.Synchronous, action, dataSource1, dataSource2, dataSource3);
		}

		#endregion

		#region Set Debug Name

		public static TPipe SetName<TPipe>(this TPipe pipe, string name)
			where TPipe : IPipe
		{
			pipe.DebugName = name;
			return pipe;
		}

		#endregion

		#region Wrap With Handover

		/// <summary>
		/// Add action to occur at the start and end of a pipe's execution.
		/// Handovers are blocking. Multiple handovers cannot happen at the same time to prevent dead-locks.
		/// (So unless you want single threaded behaviour, keep your handover actions small.)
		/// </summary>
		public static IPipeHandoverWrapper<TOutput> AddHandover<TOutput>(this IPipe<TOutput> pipe, Action<TOutput> onClaim, Action<TOutput> onRelease)
		{
			return new PipeHandoverWrapper<TOutput>(pipe, onClaim, onRelease);
		}

		#endregion

		#region Exceptions

		internal static InvalidOperationException GetPipeException(this IPipeDataSource pipe, string errorMessage)
		{
			return new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Pipe Error: [{0}] {1}", pipe, errorMessage));
		}

		#endregion
	}
}
