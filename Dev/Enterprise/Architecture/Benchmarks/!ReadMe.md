# ZArchitecture Micro Benchmark Framework

This solution contains:
1. The ZArchitecture Micro Benchmark Framework, which can be used by any CargoWise team to get benchmark numbers for their code.
2. Benchmark unit tests which cover several core ZArchitecture operations (eg: registry, business objects, factory load / save).

For further information, see https://devops.wisetechglobal.com/wtg/CargoWise/_wiki/wikis/CargoWise.wiki/16081/Micro-Benchmark-Framework


## ⚠️ Caveats

These aren't great benchmarks for the following reasons, but they are (slightly) better than nothing:
* Tests run in DEBUG builds only. The JIT may not apply all optimizations; and we have a stack of extra DEBUG only code.
* Tests don't run or fail on DAT if there is a performance regression.
* No memory metrics; CPU duration only.

⚠️ At best, these will be accurate to within an order of magnitude (10x). ⚠️
Don't expect BenchmarkDotNet level accuracy, precision or detail.

This framework focuses on micro benchmarks, that is, small operations which run in under 100ms.
It will work for longer running operations, but you will need to change the configuration. 



## Example

Taken from `BusinessObjectFactoryTests.TestInsertBusinessObject_Performance()`.

```csharp
// Arrange: Do one time setup at the start of the test
int counter = 1;

// Act: Run the benchmark!
var runner = new BenchmarkRunner(BenchmarkConfig.Default(), RunMethod);
runner.Run(
		RunBenchmarkMethod,
		// setup() and tearDown() run per iteration, but are not part of timings
		setup: () => new BusinessObjectFactory()
	)
	.ReportSummaryOnLocal();   // Saves details to a temp folder, and fails on local (not DAT) with a summary of results.

// Assert: in future, we could add a check against a baseline, with a fudge factor.

object RunBenchmarkMethod(object param)
{
	// The result of setup() is passed as param
	// One time setup is available because this is a local method
	var factory = (BusinessObjectFactory)param;

	// The object of this benchmark is saving a single new bizo.
	var data = factory.New<StmData>();
	data.SD_Name = "Benchmark" + counter;
	data.SD_GuidValue = ZGuid.NewZGuid();
	factory.Save();

	++counter;
	return data;	// Best practice is to return a value, to prevent the JIT from optimizing away the entire benchmark.
}
```

## Previous Work with Benchmarks

https://devops.wisetechglobal.com/wtg/InternalTools/_git/CWBenchmarks
