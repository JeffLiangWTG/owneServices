# IncidentMetricsSubscriberDataSchema Changelog
(See [example](../ExampleChangelog.md) for how to update this document)

## [v3] - 2024-10-20

### Removed

- Columns
	- `IME_Sequence`

## [v2] - 2024-10-01

### Added

- Columns
	- `IME_MetricCount`
	- `IME_SystemCreateUser`
	- `IME_SystemLastEditTimeUtc`
	- `IME_SystemLastEditUser`

### Removed

- Columns
	- `IME_MetricDescription`

## [v1] - 2024-09-18

### Initial schema

- Columns
	- `IME_PK`
	- `IME_CalculatedMetric`
	- `IME_EndTimeUtc`
	- `IME_IncidentNumber`
	- `IME_MetricCode`
	- `IME_MetricDescription`
	- `IME_Sequence`
	- `IME_StartTimeUtc`
	- `IME_SystemCreateTimeUtc`
